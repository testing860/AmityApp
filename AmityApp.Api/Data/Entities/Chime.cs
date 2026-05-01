using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AmityApp.Api.Data.Entities
{
    public class Chime
    {
        [Key]
        public Guid Id { get; set; }

        public Guid ForUserId { get; set; }

        [ForeignKey(nameof(ForUserId))]
        public virtual User User { get; set; }

        public Guid? FromUserId { get; set; }

        [ForeignKey(nameof(FromUserId))]
        public virtual User? FromUser { get; set; }

        [Required]
        public DateTime When { get; set; }

        public Guid? CordialId { get; set; }
        public virtual Cordial? Cordial { get; set; }

        [MaxLength(150)]
        public string Text { get; set; }
    }
}