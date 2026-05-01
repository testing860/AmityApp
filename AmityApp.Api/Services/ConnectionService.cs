using AmityApp.Api.Data;
using AmityApp.Api.Data.Entities;
using AmityApp.Api.Hubs;
using AmityApp.Shared.Dtos;
using AmityApp.Shared.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace AmityApp.Api.Services;

public class ConnectionService
{
    private readonly AmityDbContext _context;
    private readonly IHubContext<NotificationsHub, INotificationsHubClient> _hubContext;

    public ConnectionService(AmityDbContext context, IHubContext<NotificationsHub, INotificationsHubClient> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    public async Task<ApiResult> SendRequest(Guid requesterId, Guid accepterId)
    {
        if (requesterId == accepterId)
            return ApiResult.Failure("You cannot connect to yourself, you little narcissist.");

        var existing = await _context.Connections
            .FirstOrDefaultAsync(c =>
                (c.RequesterUserId == requesterId && c.AccepterUserId == accepterId) ||
                (c.RequesterUserId == accepterId && c.AccepterUserId == requesterId));

        if (existing != null)
        {
            if (existing.Status == "Pending")
                return ApiResult.Failure("A connection request is already pending.");
            if (existing.Status == "Accepted")
                return ApiResult.Failure("You are already connected.");
            _context.Connections.Remove(existing);
            await _context.SaveChangesAsync();
        }

        var connection = new Connection
        {
            RequesterUserId = requesterId,
            AccepterUserId = accepterId,
            Status = "Pending",
            RequestedAt = DateTime.Now
        };
        _context.Connections.Add(connection);
        await _context.SaveChangesAsync();

        var requester = await _context.Users.FindAsync(requesterId);
        var chime = new ChimeDto(accepterId,
            $"{requester?.Name} wants to connect with you",
            DateTime.Now,
            null,
            requesterId,
            requester?.PhotoUrl);

        await SaveChimeAsync(chime);
        await _hubContext.Clients.All.ChimeGenerated(chime);
        return ApiResult.Success();
    }

    public async Task<ApiResult> AcceptRequest(Guid connectionId, Guid currentUserId)
    {
        var conn = await _context.Connections.FindAsync(connectionId);
        if (conn == null || conn.AccepterUserId != currentUserId)
            return ApiResult.Failure("Invalid connection");

        conn.Status = "Accepted";
        conn.ResolvedAt = DateTime.Now;
        await _context.SaveChangesAsync();

        var accepter = await _context.Users.FindAsync(currentUserId);
        var chime = new ChimeDto(conn.RequesterUserId,
            $"{accepter?.Name} has accepted your connection request",
            DateTime.Now,
            null,
            currentUserId,
                accepter?.PhotoUrl);

        await SaveChimeAsync(chime);
        await _hubContext.Clients.All.ChimeGenerated(chime);
        return ApiResult.Success();
    }

    public async Task<ApiResult> RejectRequest(Guid connectionId, Guid currentUserId)
    {
        var conn = await _context.Connections.FindAsync(connectionId);
        if (conn == null || conn.AccepterUserId != currentUserId)
            return ApiResult.Failure("Invalid connection");

        conn.Status = "Rejected";
        conn.ResolvedAt = DateTime.Now;
        await _context.SaveChangesAsync();
        return ApiResult.Success();
    }

    public async Task<List<ConnectionDto>> GetPendingRequests(Guid userId)
    {
        var list = await _context.Connections
            .Include(c => c.RequesterUser)
            .Where(c => c.AccepterUserId == userId && c.Status == "Pending")
            .Select(c => new ConnectionDto
            {
                ConnectionId = c.Id,
                RequesterUserId = c.RequesterUserId,
                RequesterName = c.RequesterUser.Name,
                RequesterPhotoUrl = c.RequesterUser.PhotoUrl ?? "",
                RequestedAt = c.RequestedAt
            })
            .ToListAsync();
        return list;
    }

    public async Task<List<ConnectionDto>> GetAcceptedConnections(Guid userId)
    {
        var list = await _context.Connections
            .Include(c => c.RequesterUser)
            .Include(c => c.AccepterUser)
            .Where(c => c.Status == "Accepted" &&
                        (c.RequesterUserId == userId || c.AccepterUserId == userId))
            .Select(c => new ConnectionDto
            {
                ConnectionId = c.Id,
                RequesterUserId = c.RequesterUserId,
                AccepterUserId = c.AccepterUserId,
                RequesterName = c.RequesterUserId == userId ? c.AccepterUser.Name : c.RequesterUser.Name,
                RequesterPhotoUrl = c.RequesterUserId == userId ? c.AccepterUser.PhotoUrl : c.RequesterUser.PhotoUrl,
                RequestedAt = c.RequestedAt
            })
            .ToListAsync();
        return list;
    }

    public async Task<ApiResult> DeleteConnection(Guid connectionId, Guid currentUserId)
    {
        var conn = await _context.Connections.FindAsync(connectionId);
        if (conn == null || (conn.RequesterUserId != currentUserId && conn.AccepterUserId != currentUserId))
            return ApiResult.Failure("Connection not found");
        _context.Connections.Remove(conn);
        await _context.SaveChangesAsync();
        return ApiResult.Success();
    }

    public async Task SaveChimeAsync(ChimeDto dto)
    {
        var chime = new Chime
        {
            ForUserId = dto.ForUserId,
            CordialId = dto.CordialId,
            Text = dto.Text,
            When = dto.When,
            FromUserId = dto.FromUserId
        };
        _context.Chimes.Add(chime);
        await _context.SaveChangesAsync();
    }
}