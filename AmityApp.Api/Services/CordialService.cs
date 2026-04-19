using AmityApp.Api.Data;
using AmityApp.Api.Data.Entities;
using AmityApp.Shared.Dtos;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace AmityApp.Api.Services;
    public class CordialService
    {
    private readonly AmityDbContext _context;
    private readonly PhotoUploadService _photoUploadService;

    public CordialService(AmityDbContext context, PhotoUploadService photoUploadService)

    {
        _context = context;
        _photoUploadService = photoUploadService;
    }

    public async Task<ApiResult> SaveCordialAsync(SaveCordialDto dto, Guid userId)
    {
        string? _existingPhotoPath = null;
        if (dto.CordialId == default)
        {
            // New Cordial
            var cordial = new Cordial
            {
                Content = dto.Content,
                PostedOn = DateTime.Now,
                UserId = userId
            };
            // Save the Image (if provided)
            if (dto.Photo is not null)
            {
                (cordial.PhotoPath, cordial.PhotoUrl) = await _photoUploadService.SavePhotoAsync (dto.Photo, "uploads", "images", "users", userId.ToString(), "cordials");
            }
            _context.Cordials.Add(cordial);
        }
        else
        {
            // Existing Cordial
            var cordial = await _context.Cordials.FindAsync(dto.CordialId);
            if (cordial is null)
                return ApiResult.Failure("Ouch! Cordial does not exist");

            if (cordial.UserId != userId)
                return ApiResult.Failure("You can only edit your own cordial");

            cordial.Content = dto.Content;
            cordial.EditedOn = DateTime.UtcNow;


            // Photo 
            // 1. No CHange in Photo
            // 2. User changed Photo (Remove existing photo and upload/save new photo)
            // 3. User added Photo (previously no Photo in the Cordial - only text)
            // 4. User removed the Photo

            if (dto.Photo is not null)
            {
             _existingPhotoPath = cordial.PhotoPath;

                // User has selected a photo
                // Case 2 - User changing photo
                // Case 3 - User is adding a new photo

                 // Upload Photo
                // Update Database
                 // Remove existing photo (only for case 2)

             (cordial.PhotoPath, cordial.PhotoUrl) = await _photoUploadService.SavePhotoAsync(dto.Photo, "uploads", "images", "users", userId.ToString(), "cordials");


            }
            else
            {
                if(dto.ExistingPhotoRemoved)
                {
                    // Case 4
                    // Remove existing photo from path
                    // Update db
                    _existingPhotoPath = cordial.PhotoPath;
                    cordial.PhotoPath = null;
                    cordial.PhotoUrl = null;
                }
            }
            _context.Cordials.Update(cordial);
        }
        try
        {
            await _context.SaveChangesAsync();
            if (!string.IsNullOrWhiteSpace(_existingPhotoPath) && File.Exists(_existingPhotoPath))
            {
                File.Delete(_existingPhotoPath);
            }

            return ApiResult.Success();
        }
        catch (Exception ex)
        {
            return ApiResult.Failure(ex.Message);
        }
    }

    public async Task<CordialDto[]> GetCordialsAsync(int startIndex, int pageSize, Guid currentUserId)
    {
        var cordials = await _context.Set<CordialDto>()
             .FromSqlInterpolated($"EXEC GetCordials @StartIndex={startIndex}, @PageSize={pageSize}, @CurrentUserId={currentUserId}")
             .ToArrayAsync();

        return cordials;
    }

    public async Task<ApiResult<CommentDto>> SaveCommentAsync(SaveCommentDto dto, LoggedInUser currentUser)
    {

        Comment? comment = null;
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

    public async Task<ApiResult> ToggleCandleAsync(Guid cordialId, Guid currentUserId)
    {
        var cordialExists = await _context.Cordials.AnyAsync(c => c.Id == cordialId);
        if (!cordialExists)
            return ApiResult.Failure("Cordial not found");

        try
        {
            var candle = await _context.Candles.FirstOrDefaultAsync(c => c.CordialId == cordialId && c.UserId == currentUserId);
            if (candle is null)
            {
                candle = new Candle
                {
                    CordialId = cordialId,
                    UserId = currentUserId
                };
                _context.Candles.Add(candle);
            }
            else
            {
                _context.Candles.Remove(candle);
            }

            await _context.SaveChangesAsync();
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

            _context.Cordials.Remove(cordial);
            await _context.SaveChangesAsync();
            return ApiResult.Success();
        }
        catch (Exception ex)
        {
            return ApiResult.Failure(ex.Message);
        }
    }

}
