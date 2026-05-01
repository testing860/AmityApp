namespace AmityApp.Api.Data.Entities
{
    public class Connection
    {
        public Guid Id { get; set; }
        public Guid RequesterUserId { get; set; }
        public User RequesterUser { get; set; }
        public Guid AccepterUserId { get; set; }
        public User AccepterUser { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ResolvedAt { get; set; }
    }
}
