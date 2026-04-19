using AmityApp.Api;
using AmityApp.Api.Services;
using AmityApp.Shared.Dtos;
using System.Security.Claims;

namespace SocialMediaMaui.Api.Endpoints;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var userGroup = app.MapGroup("/api/user")
                        .RequireAuthorization()
                        .WithTags("User");

        userGroup.MapPost("/change-photo", async (IFormFile photo, UserService userService, ClaimsPrincipal principal) =>
            Results.Ok(await userService.ChangePhotoAsync(photo, principal.GetUserId())))
            .DisableAntiforgery()
            .Produces<ApiResult>()
            .WithName("ChangePhoto");

        userGroup.MapGet("/cordials", async (int startIndex, int pageSize, UserService userService, ClaimsPrincipal principal) =>
            Results.Ok(await userService.GetUserCordialsAsync(startIndex, pageSize, principal.GetUserId())))
            .Produces<CordialDto[]>()
            .WithName("GetUserCordials");

        userGroup.MapGet("/crowned-cordials", async (int startIndex, int pageSize, UserService userService, ClaimsPrincipal principal) =>
            Results.Ok(await userService.GetUserCrownedCordialsAsync(startIndex, pageSize, principal.GetUserId())))
            .Produces<CordialDto[]>()
            .WithName("GetUserCrownedCordials");

        return app;
    }
}