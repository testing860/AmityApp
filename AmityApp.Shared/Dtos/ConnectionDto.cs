using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmityApp.Shared.Dtos;

public class ConnectionDto
{
    public Guid ConnectionId { get; set; }
    public Guid RequesterUserId { get; set; }
    public Guid? AccepterUserId { get; set; }
    public string RequesterName { get; set; } = string.Empty;
    public string? RequesterPhotoUrl { get; set; }
    public DateTime RequestedAt { get; set; }
}
