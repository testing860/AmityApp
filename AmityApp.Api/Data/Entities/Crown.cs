namespace AmityApp.Api.Data.Entities
{
    public class Crown
    {
        public Guid CordialId { get; set; }
        public virtual Cordial Cordial { get; set; }
        public Guid UserId { get; set; }
        public virtual User User { get; set; }

    }
}
