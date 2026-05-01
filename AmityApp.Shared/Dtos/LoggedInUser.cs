namespace AmityApp.Shared.Dtos;

public record LoggedInUser (Guid Id, string Name, string Email, string? PhotoUrl)
{
    public string Photo => string.IsNullOrWhiteSpace(PhotoUrl) ? "user.png" : PhotoUrl;
}