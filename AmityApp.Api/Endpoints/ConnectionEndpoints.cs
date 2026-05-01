using AmityApp.Api.Services;
using AmityApp.Shared.Dtos;
using System.Security.Claims;

namespace AmityApp.Api.Endpoints;

public static class ConnectionsEndpoints
{
    public static IEndpointRouteBuilder MapConnectionsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/connections")
            .RequireAuthorization()
            .WithTags("Connections");

        group.MapPost("/request/{accepterId}", async (Guid accepterId,
            ClaimsPrincipal user, ConnectionService service) =>
        {
            var result = await service.SendRequest(user.GetUserId(), accepterId);
            return Results.Ok(result);   // always return 200 To Allow Cleint to Read.Error always
        });

        group.MapPost("/accept/{connectionId}", async (Guid connectionId,
            ClaimsPrincipal user, ConnectionService service) =>
        {
            var result = await service.AcceptRequest(connectionId, user.GetUserId());
            return Results.Ok(result);
        });

        group.MapPost("/reject/{connectionId}", async (Guid connectionId,
            ClaimsPrincipal user, ConnectionService service) =>
        {
            var result = await service.RejectRequest(connectionId, user.GetUserId());
            return Results.Ok(result);
        });

        group.MapGet("/pending", async (ClaimsPrincipal user, ConnectionService service) =>
            Results.Ok(await service.GetPendingRequests(user.GetUserId())));

        group.MapGet("/accepted", async (ClaimsPrincipal user, ConnectionService service) =>
            Results.Ok(await service.GetAcceptedConnections(user.GetUserId())));

        group.MapDelete("/{connectionId}", async (Guid connectionId, ClaimsPrincipal user, ConnectionService service) =>
        {
            var result = await service.DeleteConnection(connectionId, user.GetUserId());
            return Results.Ok(result);
        });

        return app;
    }
}