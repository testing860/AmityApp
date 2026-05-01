using AmityApp.Api.Data;
using AmityApp.Api.Data.Entities;
using AmityApp.Api.Hubs;
using AmityApp.Shared.Dtos;
using AmityApp.Shared.Hubs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace AmityApp.Api.Services;
    public class CordialService
    {
    private readonly AmityDbContext _context;
    private readonly PhotoUploadService _photoUploadService;
    private readonly IHubContext<NotificationsHub, INotificationsHubClient> _hubContext;
    private readonly ConnectionService _connectionService;

    public CordialService(AmityDbContext context, PhotoUploadService photoUploadService, 
        IHubContext<NotificationsHub, INotificationsHubClient> hubContext, ConnectionService connectionService)

    {
        _context = context;
        _photoUploadService = photoUploadService;
        _hubContext = hubContext;
        _connectionService = connectionService;
    }

    public async Task<ApiResult<CordialDto>> SaveCordialAsync(SaveCordialDto dto, LoggedInUser user)
    {
        string? _existingPhotoPath = null;
        Cordial? cordial = null;
        bool sendChime = false;
        if (dto.CordialId == default)
        {
            cordial = new Cordial
            {
                Content = dto.Content,
                PostedOn = DateTime.Now,
                UserId = user.Id,
                Vibe = dto.Vibe,
                Visibility = dto.Visibility ?? "Public"
            };
            if (dto.Photo is not null)
            {
                (cordial.PhotoPath, cordial.PhotoUrl) = await _photoUploadService.SavePhotoAsync (dto.Photo, "uploads", "images", "users", user.Id.ToString(), "cordials");
            }
            _context.Cordials.Add(cordial);
        }
        else
        {
            cordial = await _context.Cordials.FindAsync(dto.CordialId);
            if (cordial is null)
                return ApiResult<CordialDto>.Failure("Ouch! Cordial does not exist");

            if (cordial.UserId != user.Id)
                return ApiResult<CordialDto>.Failure("You can only edit your own cordial");

            cordial.Content = dto.Content;
            cordial.Vibe = dto.Vibe;
            cordial.Visibility = dto.Visibility ?? "Public";
            cordial.EditedOn = DateTime.Now;

            if (dto.Photo is not null)
            {
             _existingPhotoPath = cordial.PhotoPath;

             (cordial.PhotoPath, cordial.PhotoUrl) = await _photoUploadService.SavePhotoAsync(dto.Photo, "uploads", "images", "users", user.Id.ToString(), "cordials");


            }
            else
            {
                if(dto.ExistingPhotoRemoved)
                {
                    _existingPhotoPath = cordial.PhotoPath;
                    cordial.PhotoPath = null;
                    cordial.PhotoUrl = null;
                }
            }
            _context.Cordials.Update(cordial);
            sendChime = true;
        }
        try
        {
            await _context.SaveChangesAsync();

            if (!string.IsNullOrWhiteSpace(_existingPhotoPath) && File.Exists(_existingPhotoPath))
            {
                File.Delete(_existingPhotoPath);
            }
            var cordialDto = new CordialDto
            {
                Content = cordial.Content,
                Vibe = cordial.Vibe,
                Visibility = cordial.Visibility,
                EditedOn = cordial.EditedOn,
                PhotoUrl = cordial.PhotoUrl,
                CordialId = cordial.Id,
                UserId = cordial.UserId,
                UserName = user.Name,
                UserPhotoUrl = user.PhotoUrl,
                PostedOn = cordial.PostedOn
            };

            if (sendChime)
            {
                await _hubContext.Clients.All.CordialChanged(cordialDto);
            }

            return ApiResult<CordialDto>.Success(cordialDto);
        }
        catch (Exception ex)
        {
            return ApiResult<CordialDto>.Failure(ex.Message);
        }
    }

    public async Task<CordialDto[]> GetCordialsAsync(int startIndex, int pageSize, Guid currentUserId)
    {
        var cordials = await _context.Set<CordialDto>()
             .FromSqlInterpolated($"EXEC GetCordials @StartIndex={startIndex}, @PageSize={pageSize}, @CurrentUserId={currentUserId}")
             .ToArrayAsync();

        return cordials;
    }

    public async Task<CordialDto?> GetCordialAsync(Guid cordialId, Guid currentUserId)
    {
        var cordials = await _context.Set<CordialDto>()
        .FromSqlInterpolated($"EXEC GetCordialById @CordialId={cordialId}, @CurrentUserId = {currentUserId}")
        .ToArrayAsync();

        if (cordials.Length == 0)
            return null;
        return cordials[0];
    }

    public async Task<ApiResult<CommentDto>> SaveCommentAsync(SaveCommentDto dto, LoggedInUser currentUser)
    {
        var cordialOwnerId = await _context.Cordials.Where(c => c.Id == dto.CordialId)
                                            .Select(c => c.UserId).FirstOrDefaultAsync();
        if (cordialOwnerId == default)
            return ApiResult<CommentDto>.Failure("Cordial not found");

        Comment? comment = null;
        bool sendChime = false;


        if (dto.CommentId == Guid.Empty)
        {
            // New Comment
            comment = new Comment
            {
                CordialId = dto.CordialId,
                UserId = currentUser.Id,
                Content = dto.Content,
                CommentedOn = DateTime.Now
            };
            _context.Comments.Add(comment);
            sendChime = true;
        }
        else
        {
            // Existing Comment
            comment = await _context.Comments.FindAsync(dto.CommentId);
            if (comment is null)
                return ApiResult<CommentDto>.Failure("Comment not found");

            if (comment.UserId != currentUser.Id)
                return ApiResult<CommentDto>.Failure("You can modify your own comments only");

            comment.Content = dto.Content;
            _context.Comments.Update(comment);
        }
        try
        {
            await _context.SaveChangesAsync();

            var commentDto = new CommentDto
            {
                CommentedOn = comment.CommentedOn,
                CommentId = comment.Id,
                Content = comment.Content,
                CordialId = comment.CordialId,
                UserId = currentUser.Id,
                UserName = currentUser.Name,
                UserPhotoUrl = currentUser.PhotoUrl,
            };

            if (sendChime)
            {
                var chimeDto = new ChimeDto(cordialOwnerId, $"{currentUser.Name} commented on your cordial!",
                    DateTime.Now, dto.CordialId, currentUser.Id, currentUser.PhotoUrl);
                await _connectionService.SaveChimeAsync(chimeDto);
                await _hubContext.Clients.All.CommentAddedToCordial(commentDto);
                await _hubContext.Clients.All.ChimeGenerated(chimeDto);
            }

            return ApiResult<CommentDto>.Success(commentDto);
        }
        catch (Exception ex)
        {

            return ApiResult<CommentDto>.Failure(ex.Message);
        }
    }

    public async Task<CommentDto[]> GetCordialCommentsAsync(Guid cordialId, int startIndex, int pageSize) =>
    await _context.Comments
    .Where(c => c.CordialId == cordialId)
    .OrderByDescending(c => c.CommentedOn)
    .Skip(startIndex)
    .Take(pageSize)
    .Select(c => new CommentDto
    {
        CommentedOn = c.CommentedOn,
        CommentId = c.Id,
        Content = c.Content,
        CordialId = cordialId,
        UserId = c.UserId,
        UserName = c.User.Name,
        UserPhotoUrl = c.User.PhotoUrl
    })
    .ToArrayAsync();

    public async Task<ApiResult> ToggleCandleAsync(Guid cordialId, LoggedInUser currentUser)
    {
        var cordialOwnerId = await _context.Cordials.Where(c => c.Id == cordialId)
                                                    .Select(c => c.UserId).FirstOrDefaultAsync();
        if (cordialOwnerId == default)
            return ApiResult.Failure("Cordial not found");

        try
        {
            bool sendChime = false;
            var candle = await _context.Candles.FirstOrDefaultAsync(c => c.CordialId == cordialId && c.UserId == currentUser.Id);
            if (candle is null)
            {
                candle = new Candle
                {
                    CordialId = cordialId,
                    UserId = currentUser.Id
                };
                _context.Candles.Add(candle);
                sendChime = true;
            }
            else
            {
                _context.Candles.Remove(candle);
            }

            await _context.SaveChangesAsync();

            if(sendChime)
            {
                var chimeDto = new ChimeDto(cordialOwnerId, $"{currentUser.Name} has lit a candle on your cordial",
                    DateTime.Now, cordialId, currentUser.Id, currentUser.PhotoUrl);
                await _connectionService.SaveChimeAsync(chimeDto);
                await _hubContext.Clients.All.ChimeGenerated(chimeDto);
            }
            return ApiResult.Success();
        }
        catch (Exception ex)
        {
            return ApiResult.Failure(ex.Message);
        }
    }

    public async Task<ApiResult> ToggleCrownAsync(Guid cordialId, Guid currentUserId)
    {
        var cordialExists = await _context.Cordials.AnyAsync(c => c.Id == cordialId);
        if (!cordialExists)
            return ApiResult.Failure("Cordial not found");

        try
        {
            var crown = await _context.Crowns.FirstOrDefaultAsync(c => c.CordialId == cordialId && c.UserId == currentUserId);
            if (crown is null)
            {
                crown = new Crown
                {
                    CordialId = cordialId,
                    UserId = currentUserId
                };
                _context.Crowns.Add(crown);
            }
            else
            {
                _context.Crowns.Remove(crown);
            }

            await _context.SaveChangesAsync();
            return ApiResult.Success();
        }
        catch (Exception ex)
        {
            return ApiResult.Failure(ex.Message);
        }
    }

    public async Task<ApiResult> DeleteCordialAsync(Guid cordialId, Guid currentUserId)
    {
        try
        {
            var cordial = await _context.Cordials.FindAsync(cordialId);
            if (cordial is null)
                return ApiResult.Failure("Cordial not found");

            if (cordial.UserId != currentUserId)
                return ApiResult.Failure("You can delete your own cordials only");

            cordial.IsDeleted = true;
            _context.Cordials.Update(cordial);

            await _context.SaveChangesAsync();
                await _hubContext.Clients.All.CordialDeleted(cordialId);

            return ApiResult.Success();
        }
        catch (Exception ex)
        {
            return ApiResult.Failure(ex.Message);
        }
    }

    public async Task<CordialDto[]> GetOnlyConnectionCordialsAsync(int startIndex, int pageSize, Guid currentUserId)
    {
        return await _context.Set<CordialDto>()
            .FromSqlInterpolated($"EXEC GetOnlyConnectionCordials @StartIndex={startIndex}, @PageSize={pageSize}, @CurrentUserId={currentUserId}")
            .ToArrayAsync();
    }


}
