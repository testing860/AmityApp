using AmityApp.Api.Data;
using AmityApp.Shared.Hubs;
using AmityApp.Shared.Dtos;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using AmityApp.Api.Hubs;

namespace AmityApp.Api.Services;

public class UserService
{
    private readonly AmityDbContext _context;
    private readonly PhotoUploadService _photoUploadService;
    private readonly IHubContext<NotificationsHub, INotificationsHubClient> _hubContext;

    public UserService(AmityDbContext context, PhotoUploadService photoUploadService, IHubContext<NotificationsHub, INotificationsHubClient> hubContext)
    {
        _context = context;
        _photoUploadService = photoUploadService;
        _hubContext = hubContext;
    }

    public async Task<ApiResult<string>> ChangePhotoAsync(IFormFile photo, Guid currentUserId)
    {
        var user = await _context.Users.FindAsync(currentUserId);
        if (user is null)
        {
            return ApiResult<string>.Failure("User not found");
        }

        try
        {
            var existingPhotoPath = user.PhotoPath;

            (user.PhotoPath, user.PhotoUrl) = await _photoUploadService.SavePhotoAsync(photo, "uploads", "images", "users");

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.UserPhotoChanged(new UserPhotoChangedDto(currentUserId, user.PhotoUrl));

            if (string.IsNullOrWhiteSpace(existingPhotoPath) && File.Exists(existingPhotoPath))
            {
                File.Delete(existingPhotoPath);
            }
            return ApiResult<string>.Success(user.PhotoUrl);
        }
        catch (Exception ex)
        {

            return ApiResult<string>.Failure(ex.Message);
        }
    }

    public async Task<CordialDto[]> GetUserCordialsAsync(int startIndex, int pageSize, Guid currentUserId)
    {
        var cordials = await _context.Set<CordialDto>()
             .FromSqlInterpolated($"EXEC GetUserCordials @StartIndex={startIndex}, @PageSize={pageSize}, @CurrentUserId={currentUserId}")
             .ToArrayAsync();

        return cordials;
    }

    public async Task<CordialDto[]> GetUserCrownedCordialsAsync(int startIndex, int pageSize, Guid currentUserId)
    {
        var cordials = await _context.Set<CordialDto>()
             .FromSqlInterpolated($"EXEC GetUserChimedCordials @StartIndex={startIndex}, @PageSize={pageSize}, @CurrentUserId={currentUserId}")
             .ToArrayAsync();
        return cordials;
    }

    public async Task<ChimeDto[]> GetChimesAsync(int startIndex, int pageSize, Guid currentUserId)
    {
        return await _context.Chimes
            .Include(c => c.FromUser)
            .Where(c => c.ForUserId == currentUserId)
            .OrderByDescending(c => c.When)
            .Skip(startIndex)
            .Take(pageSize)
            .Select(c => new ChimeDto(
                c.ForUserId,
                c.Text,
                c.When,
                c.CordialId,
                c.FromUserId,
                c.FromUser!.PhotoUrl))
            .ToArrayAsync();
    }

    public async Task<UserDto?> FindUserByEmailAsync(string email)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null) return null;
        return new UserDto { Id = user.Id, Name = user.Name, Email = user.Email, PhotoUrl = user.PhotoUrl };
    }
}