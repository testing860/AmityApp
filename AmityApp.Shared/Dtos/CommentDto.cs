namespace AmityApp.Shared.Dtos;

    public class CommentDto
    {

    private const string ApiBaseUrl = "http://10.0.2.2:5009/";
    public Guid CordialId { get; set; }
        public Guid CommentId { get; set; }
        public string Content { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string? UserPhotoUrl { get; set; }

        public DateTime CommentedOn { get; set; }

    // Local Computed Properties
    public string? FullUserPhotoUrl
    {
        get
        {
            if (string.IsNullOrWhiteSpace(UserPhotoUrl))
                return "user.png";

            var url = UserPhotoUrl;

            url = url.Replace("https://localhost:7134", "http://10.0.2.2:5009")
                     .Replace("https://10.0.2.2:7134", "http://10.0.2.2:5009");

            if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                url = $"{ApiBaseUrl}{url.Replace("\\", "/")}";
            }

            return url;
        }
    }
}

