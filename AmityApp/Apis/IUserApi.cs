using AmityApp.Shared.Dtos;
using Refit;

namespace AmityApp.Apis;

public interface IUserApi
{
    [Multipart]
    [Post("/api/user/change-photo")]
    Task<ApiResult<string>> ChangePhotoAsync([AliasAs("photo")] StreamPart photo);

    [Get("/api/user/cordials")]
    Task<CordialDto[]> GetUserCordialsAsync([Query] int startIndex, [Query] int pageSize);

    [Get("/api/user/crowned-cordials")]
    Task<CordialDto[]> GetUserCrownedCordialsAsync([Query] int startIndex, [Query] int pageSize);

    [Get("/api/user/chimes")]
    Task<ChimeDto[]> GetChimesAsync(int startIndex, int pageSize);

    [Get("/api/user/search")]
    Task<ApiResult<UserDto?>> SearchUserAsync([Query] string email);
}