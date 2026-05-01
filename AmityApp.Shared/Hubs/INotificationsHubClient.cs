using AmityApp.Shared.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmityApp.Shared.Hubs;

public interface INotificationsHubClient
{
    Task CordialChanged(CordialDto cordial);
    Task CordialDeleted(Guid cordialId);
    Task CommentAddedToCordial(CommentDto comment);
    Task UserPhotoChanged(UserPhotoChangedDto dto);
    Task ChimeGenerated(ChimeDto chime);
}

public record struct UserPhotoChangedDto(Guid UserId, string? PhotoUrl);
