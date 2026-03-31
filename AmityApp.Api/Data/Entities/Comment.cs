using System.ComponentModel.DataAnnotations;

namespace AmityApp.Api.Data.Entities
{
    public class Comment

    {

        [Key]
        public Guid Id { get; set; }
        public Guid CordialId { get; set; }
        public virtual Cordial Cordial { get; set; }
        public string Content { get; set; }

        public Guid UserId { get; set; }

        public virtual User User { get; set; }

        public DateTime CommentedOn { get; set; }
        public DateTime? EditedOn { get; set; }

    }
}
