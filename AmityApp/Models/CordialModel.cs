using AmityApp.Shared.Dtos;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Maui.ApplicationModel.DataTransfer;
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

    [ObservableProperty, NotifyPropertyChangedFor(nameof(FullUserPhotoUrl), nameof(CordialTemplateContentViewName))]
    private string? _userPhotoUrl;

    [ObservableProperty, NotifyPropertyChangedFor(nameof(FullPhotoUrl), nameof(CordialTemplateContentViewName))]
    private string? _photoUrl;

    [ObservableProperty, NotifyPropertyChangedFor(nameof(CordialTemplateContentViewName))]
    private string? _content;

    [ObservableProperty, NotifyPropertyChangedFor(nameof(HasVibe), nameof(VibeIcon),
        nameof(VibeDisplay), nameof(VibeColor))]
    private string? _vibe;

    public bool HasVibe => !string.IsNullOrEmpty(Vibe);

    // Choose icon based on vibe value
    public string? VibeIcon => _vibe switch
    {
        "love" => "vibes/love.svg",
        "gratitude" => "vibes/gratitude.svg",
        "apology" => "vibes/apology.svg",
        "appreciation" => "vibes/appreciation.svg",
        "mindfulness" => "vibes/mindfulness.svg",
        "thank_you" => "vibes/thankyou.svg",
        "joy" => "vibes/joy.svg",
        "hope" => "vibes/hope.svg",
        "comfort" => "vibes/comfort.svg",
        "sorrow" => "vibes/sorrow.svg",
        _ => null
    };

    public Color VibeColor => _vibe switch
    {
        "love" => Color.FromArgb("#E74C3C"),   // red
        "gratitude" => Color.FromArgb("#D4AC0D"),   // gold
        "apology" => Color.FromArgb("#5DADE2"),   // light blue
        "appreciation" => Color.FromArgb("#F06292"),   // soft warm pink (updated)
        "mindfulness" => Color.FromArgb("#9B59B6"),   // purple
        "thank_you" => Color.FromArgb("#FF6F64"),   // coral
        "joy" => Color.FromArgb("#FF9800"),   // vibrant orange
        "hope" => Color.FromArgb("#1DB954"),   // vibrant green
        "comfort" => Color.FromArgb("#607D8B"),   // muted slate blue
        "sorrow" => Color.FromArgb("#000000"),   // black
        _ => Colors.Gray
    };

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
        Vibe = dto.Vibe,
        Visibility = dto.Visibility,
        IsCrowned = dto.IsCrowned,
        IsLit = dto.IsLit,
        PhotoUrl = dto.PhotoUrl,
        PostedOnDisplay = dto.PostedOnDisplay,
        UserId = dto.UserId,
        UserName = dto.UserName,
        UserPhotoUrl = dto.UserPhotoUrl
    };

    public void NotifyFullPhotoUrlChanged() => OnPropertyChanged(nameof(FullPhotoUrl));

    public string? VibeDisplay
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Vibe))
                return null;
            var display = Vibe.Replace("_", " ");
            if (display.Length > 0)
                display = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(display.ToLowerInvariant());
            return display;
        }
    }

    [ObservableProperty]
    private string? _visibility;

    public void Refresh()
    {
        OnPropertyChanged(nameof(PhotoUrl));
        OnPropertyChanged(nameof(Content));
        OnPropertyChanged(nameof(PostedOnDisplay));
        OnPropertyChanged(nameof(Vibe));
        OnPropertyChanged(nameof(VibeIcon));
        OnPropertyChanged(nameof(HasVibe));
        OnPropertyChanged(nameof(FullPhotoUrl));
    }
}
