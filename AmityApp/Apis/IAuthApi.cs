using AmityApp.Shared.Dtos;
using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmityApp.Apis;

public interface IAuthApi
{
    [Post("/api/auth/register")]
    Task<ApiResult<Guid>> RegisterAsync(RegisterDto dto);

    [Multipart]
    [Post("/api/auth/register/{userId}/add-photo")]
    Task<ApiResult> UploadPhotoAsync(Guid userId, StreamPart photo);

    [Post("/api/auth/login")]
    Task<ApiResult<LoginResponseDto>> LoginAsync(LoginDto dto);
}
