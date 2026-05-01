using System.ComponentModel.DataAnnotations;

namespace AmityApp.Api.Data.Entities
{
    public class Cordial

    {
        [Key]
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public virtual User User { get; set; }

        public string? Content { get; set; }

        public string? Vibe { get; set; }

        public string Visibility { get; set; } = "Public"; // Public / ConnectionsOnly
        public string? PhotoPath { get; set; }
        public string? PhotoUrl { get; set; }

        public DateTime PostedOn { get; set; }
        public DateTime? EditedOn { get; set; }

        public bool IsDeleted { get; set; }
    }
}
