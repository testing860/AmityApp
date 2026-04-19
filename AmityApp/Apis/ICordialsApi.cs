using AmityApp.Shared.Dtos;
using Refit;

namespace AmityApp.Apis;
public interface ICordialsApi
{

    [Multipart]
    [Post("/api/cordials/save")]
    Task<ApiResult> SaveCordialAsync(StreamPart? photo, string serializedSaveCordialDto);

    [Get("/api/cordials")]
    Task<CordialDto[]> GetCordialsAsync([Query] int startIndex, [Query] int pageSize);

    [Post("/api/cordials/{cordialId}/comments")]
    Task<ApiResult<CommentDto>> SaveCommentAsync(Guid cordialId, [Body] SaveCommentDto dto);

    [Get("/api/cordials/{cordialId}/comments")]
    Task<CommentDto[]> GetCordialCommentsAsync(Guid cordialId, [Query] int startIndex, [Query] int pageSize);

    [Post("/api/cordials/{cordialId}/toggle-candle")]
    Task<ApiResult> ToggleCandleAsync(Guid cordialId);

    [Post("/api/cordials/{cordialId}/toggle-crown")]
    Task<ApiResult> ToggleCrownAsync(Guid cordialId);

    [Delete("/api/cordials/{cordialId}")]
    Task<ApiResult> DeleteCordialAsync(Guid cordialId);
}
