using AmityApp.Shared.Dtos;
using Refit;

namespace AmityApp.Apis;

public interface IConnectionsApi
{
    [Post("/api/connections/request/{accepterId}")]
    Task<ApiResult> SendRequest(Guid accepterId);

    [Post("/api/connections/accept/{connectionId}")]
    Task<ApiResult> AcceptRequest(Guid connectionId);

    [Post("/api/connections/reject/{connectionId}")]
    Task<ApiResult> RejectRequest(Guid connectionId);

    [Get("/api/connections/pending")]
    Task<List<ConnectionDto>> GetPendingRequests();

    [Get("/api/connections/accepted")]
    Task<List<ConnectionDto>> GetAcceptedConnections();

    [Delete("/api/connections/{connectionId}")]
    Task<ApiResult> DeleteConnection(Guid connectionId);
}