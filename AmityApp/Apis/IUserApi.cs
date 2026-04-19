using AmityApp.Shared.Dtos;
using Refit;

namespace AmityApp.Apis;

public interface IUserApi
{
    [Multipart]
    [Post("/api/user/change-photo")]
    Task<ApiResult> ChangePhotoAsync([AliasAs("photo")] StreamPart photo);

    [Get("/api/user/cordials")]
    Task<CordialDto[]> GetUserCordialsAsync([Query] int startIndex, [Query] int pageSize);

    [Get("/api/user/crowned-cordials")]
    Task<CordialDto[]> GetUserCrownedCordialsAsync([Query] int startIndex, [Query] int pageSize);
}