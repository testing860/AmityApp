using System.Text.Json.Serialization;

namespace AmityApp.Shared.Dtos;

public class CordialDto
{
    private const string ApiBaseUrl = "http://10.0.2.2:5009/";   // Android Emulator Address

    public Guid CordialId { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; }
    public string? UserPhotoUrl { get; set; }
    public string? Content { get; set; }
    public string? PhotoUrl { get; set; }

    public DateTime PostedOn { get; set; }

    public DateTime? EditedOn { get; set; }

    [JsonIgnore]
    public DateTime PostedOnDisplay => EditedOn ?? PostedOn;

    public bool IsLit { get; set; }
    public bool IsCrowned { get; set; }

    // Local Computed Properties
    public string? FullPhotoUrl
    {
        get
        {
            if (string.IsNullOrWhiteSpace(PhotoUrl))
                return null;

            var url = PhotoUrl;

            // Replace development HTTPS URLs with HTTP emulator URL
            url = url.Replace("https://localhost:7134", "http://10.0.2.2:5009")
                     .Replace("https://10.0.2.2:7134", "http://10.0.2.2:5009");

            // If still a relative path, prepend base URL
            if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                url = $"{ApiBaseUrl}{url.Replace("\\", "/")}";
            }

            return url;
        }
    }

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

    [JsonIgnore]
    public string CordialTemplateContentViewName
    {
        get
        {
            if (string.IsNullOrWhiteSpace(PhotoUrl))
            {
                // We have only text content
                return "WithNoImage";
            }

            if (string.IsNullOrWhiteSpace(Content))
            {
                // We have only Photo/image
                return "ImageOnly";
            }
            // We have both
            return "WithImage";
        }
    }

    [JsonIgnore]
    public string IsLitIcon => IsLit ? "candle_f.png" : "candle.png";

    [JsonIgnore]
    public string IsCrownedIcon => IsCrowned ? "crown_f.png" : "crown.png";
}