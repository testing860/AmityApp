namespace AmityApp.Shared.Dtos;

public record ChimeDto(Guid ForUserId, string Text, DateTime When, Guid? CordialId, Guid? FromUserId, string? UserPhotoUrl);