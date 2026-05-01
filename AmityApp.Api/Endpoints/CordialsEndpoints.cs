using AmityApp.Api;
using AmityApp.Api.Services;
using AmityApp.Shared.Dtos;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

namespace SocialMediaMaui.Api.Endpoints;

public static class CordialsEndpoints
{
    public static IEndpointRouteBuilder MapCordialsEndpoints(this IEndpointRouteBuilder app)
    {
        var cordialsGroup = app.MapGroup("/api/cordials")
                               .RequireAuthorization()
                               .WithTags("Cordials");

        cordialsGroup.MapPost("/save", async ([FromForm] IFormFile? photo, [FromForm] string serializedSaveCordialDto, CordialService cordialService, ClaimsPrincipal principal) =>
        {
            SaveCordialDto dto = JsonSerializer.Deserialize<SaveCordialDto>(serializedSaveCordialDto)!;

            dto.Photo = photo;

            return Results.Ok(await cordialService.SaveCordialAsync(dto, principal.GetUser()));
        })
        .DisableAntiforgery()
        .Produces<ApiResult<CordialDto>>()
        .WithName("SaveCordial");

        cordialsGroup.MapGet("/", async (int startIndex, int pageSize, CordialService cordialService, ClaimsPrincipal principal) =>
            Results.Ok(await cordialService.GetCordialsAsync(startIndex, pageSize, principal.GetUserId())))
            .Produces<CordialDto[]>()
            .WithName("GetCordials");

        cordialsGroup.MapPost("/{cordialId:guid}/comments",
            async (Guid cordialId, SaveCommentDto dto, CordialService cordialService, ClaimsPrincipal principal) =>
            Results.Ok(await cordialService.SaveCommentAsync(dto, principal.GetUser())))
            .Produces<ApiResult<CommentDto>>()
            .WithName("SaveComment");

        cordialsGroup.MapGet("/{cordialId:guid}/comments", async (Guid cordialId, int startIndex, int pageSize, CordialService cordialService) =>
            Results.Ok(await cordialService.GetCordialCommentsAsync(cordialId, startIndex, pageSize)))
            .Produces<CommentDto[]>()
            .WithName("GetCordialComments");

        cordialsGroup.MapPost("/{cordialId:guid}/toggle-candle",
            async (Guid cordialId, CordialService cordialService, ClaimsPrincipal principal) =>
            Results.Ok(await cordialService.ToggleCandleAsync(cordialId, principal.GetUser())))
            .Produces<ApiResult>()
            .WithName("ToggleCandle");

        cordialsGroup.MapPost("/{cordialId:guid}/toggle-crown",
            async (Guid cordialId, CordialService cordialService, ClaimsPrincipal principal) =>
            Results.Ok(await cordialService.ToggleCrownAsync(cordialId, principal.GetUserId())))
            .Produces<ApiResult>()
            .WithName("ToggleCrown");

        cordialsGroup.MapDelete("/{cordialId:guid}", async (Guid cordialId, CordialService cordialService, ClaimsPrincipal principal) =>
            Results.Ok(await cordialService.DeleteCordialAsync(cordialId, principal.GetUserId())))
            .Produces<ApiResult>()
            .WithName("DeleteCordial");

        cordialsGroup.MapGet("/{cordialId:guid}", async (Guid cordialId, CordialService cordialService, ClaimsPrincipal principal) =>
            Results.Ok(await cordialService.GetCordialAsync(cordialId, principal.GetUserId())))
            .Produces<CordialDto>()
            .WithName("GetCordialById");

        cordialsGroup.MapGet("/connections-only", async (int startIndex, int pageSize, CordialService cordialService, ClaimsPrincipal principal) =>
            Results.Ok(await cordialService.GetOnlyConnectionCordialsAsync(startIndex, pageSize, principal.GetUserId())))
            .Produces<CordialDto[]>()
            .WithName("GetOnlyConnectionCordials");

        return app;
    }
}
