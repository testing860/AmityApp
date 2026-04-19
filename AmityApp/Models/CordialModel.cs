using AmityApp.Shared.Dtos;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AmityApp.Models;

public partial class CordialModel : ObservableObject
{
    private const string ApiBaseUrl = "http://10.0.2.2:5009/";   // Android Emulator Address

    public Guid CordialId { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; }
    public string? UserPhotoUrl { get; set; }
    public string? Content { get; set; }
    public string? PhotoUrl { get; set; }

    public DateTime PostedOnDisplay { get; set; }

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
    public string CordialTemplateContentViewName
    {
        get
        {
            if (string.IsNullOrWhiteSpace(PhotoUrl))
            {
                // Text Content Only
                return "WithNoImage";
            }

            if (string.IsNullOrWhiteSpace(Content))
            {
                // Photo Only
                return "ImageOnly";
            }
            // Text + Photo
            return "WithImage";
        }
    }



    [ObservableProperty, NotifyPropertyChangedFor(nameof(IsLitIcon))]
    private bool _isLit;

    [ObservableProperty, NotifyPropertyChangedFor(nameof(IsCrownedIcon))]
    private bool _isCrowned;

    public string IsLitIcon => IsLit ? "candle_f.png" : "candle.png";
    public string IsCrownedIcon => IsCrowned ? "crown_f.png" : "crown.png";

    public static CordialModel FromDto(CordialDto dto) =>
    new()
    {
        CordialId = dto.CordialId,
        Content = dto.Content,
        IsCrowned = dto.IsCrowned,
        IsLit = dto.IsLit,
        PhotoUrl = dto.PhotoUrl,
        PostedOnDisplay = dto.PostedOnDisplay,
        UserId = dto.UserId,
        UserName = dto.UserName,
        UserPhotoUrl = dto.UserPhotoUrl
    };

}
