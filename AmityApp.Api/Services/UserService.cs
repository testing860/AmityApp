using AmityApp.Api.Data;
using AmityApp.Shared.Dtos;
using Microsoft.EntityFrameworkCore;

namespace AmityApp.Api.Services;

public class UserService
{
    private readonly AmityDbContext _context;
    private readonly PhotoUploadService _photoUploadService;

    public UserService(AmityDbContext context, PhotoUploadService photoUploadService)
    {
        _context = context;
        _photoUploadService = photoUploadService;
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
             .FromSqlInterpolated($"EXEC GetUserCrownedCordials @StartIndex={startIndex}, @PageSize={pageSize}, @CurrentUserId={currentUserId}")
             .ToArrayAsync();

        return cordials;
    }

}