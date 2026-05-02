using AmityApp.Apis;
using AmityApp.Models;
using AmityApp.Pages;
using AmityApp.Services;
using AmityApp.Shared.Dtos;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AmityApp.ViewModels;

[QueryProperty(nameof(Cordial), nameof(Cordial))]
public partial class SaveCordialViewModel : BaseViewModel, IQueryAttributable
{
    private readonly ICordialsApi _cordialsApi;
    private readonly AuthService _authService;

    public SaveCordialViewModel(ICordialsApi cordialsApi, AuthService authService)
    {
        _cordialsApi = cordialsApi;
        _authService = authService;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue(nameof(Cordial), out var cordialObj) && cordialObj is CordialModel cordial)
        {
            Cordial = cordial;
        }
        else
        {
            Cordial = null;
        }
    }

    [ObservableProperty]
    private string _content = string.Empty;

    [ObservableProperty]
    private string _photoPath = string.Empty;

    private bool _isPhotoNew;
    private string? _existingPhotoUrl;
    public bool IsEditing => Cordial?.CordialId != Guid.Empty && Cordial?.CordialId != default;
    public string PageTitle => IsEditing ? "Edit Cordial" : "Create Cordial";

    [RelayCommand]
    private async Task SelectPhotoAsync()
    {
        var selectedPhotoSource = await ChoosePhotoAsync();
        if (!string.IsNullOrWhiteSpace(selectedPhotoSource))
        {
            PhotoPath = selectedPhotoSource;
            _isPhotoNew = true;
        }
    }

    [RelayCommand]
    private void RemovePhoto()
    {
        PhotoPath = "";
        _isPhotoNew = true;
    }

    public string UserName => _authService.User?.Name ?? "You";
    public string UserPhotoDisplayUrl
    {
        get
        {
            if (string.IsNullOrWhiteSpace(_authService.User?.PhotoUrl))
                return "user.png";

            var url = _authService.User.PhotoUrl;
            url = url.Replace("https://localhost:7134", "http://10.0.2.2:5009")
                     .Replace("https://10.0.2.2:7134", "http://10.0.2.2:5009");
            return url.StartsWith("http") ? url : $"http://10.0.2.2:5009/{url.Replace("\\", "/")}";
        }
    }

    [RelayCommand]
    private async Task SaveCordialAsync()
    {
        System.Diagnostics.Debug.WriteLine("🚀 SaveCordialAsync command FIRED!");
        if (string.IsNullOrWhiteSpace(Content) && string.IsNullOrWhiteSpace(PhotoPath))
        {
            await ToastAsync("Either content or photo is required");
            return;
        }

        await MakeApiCall(async () =>
        {
            StreamPart? photoStreamPart = null;
            var dto = new SaveCordialDto
            {
                Content = Content,
                CordialId = Cordial?.CordialId ?? default
            };

            var vibe = SelectedVibe?.ToLowerInvariant().Replace(" ", "_");
            var effectiveVibe = (vibe == "none" || string.IsNullOrEmpty(vibe)) ? null : vibe;
            dto.Vibe = effectiveVibe;
            dto.Visibility = SelectedVisibility == "Connections Only" ? "ConnectionsOnly" : "Public";

            if (_isPhotoNew)
            {
                if (!string.IsNullOrWhiteSpace(PhotoPath))
                {
                    var fileName = Path.GetFileName(PhotoPath);
                    var fileStream = File.OpenRead(PhotoPath);
                    photoStreamPart = new StreamPart(fileStream, fileName);
                }
                else
                {
                    dto.ExistingPhotoRemoved = true;
                }
            }

            var result = await _cordialsApi.SaveCordialAsync(photoStreamPart, JsonSerializer.Serialize(dto));
            if (!result.IsSuccess)
            {
                await ShowErrorAlertAsync(result.Error);
                return;
            }

            await ToastAsync("Cordial has been published successfully!");
            Content = null;
            PhotoPath = "";
            _isPhotoNew = false;

            var savedCordial = CordialModel.FromDto(result.Data);
            await NavigateAsync("..", new Dictionary<string, object>
            {
                [nameof(DetailsViewModel.Cordial)] = savedCordial
            });
        });
    }

    [ObservableProperty, NotifyPropertyChangedFor(nameof(PageTitle), nameof(IsEditing))]
    private CordialModel? _cordial;

    async partial void OnCordialChanged(CordialModel? value)
    {
        if (value is not null)
        {
            Content = value.Content;
            _existingPhotoUrl = value.PhotoUrl;

            if (!string.IsNullOrWhiteSpace(value.PhotoUrl))
            {
                var fixedUrl = GetEmulatorImageUrl(value.PhotoUrl);
                var localPreviewPath = await DownloadExistingPhotoAsync(fixedUrl);
                PhotoPath = localPreviewPath ?? string.Empty;
            }
            else
            {
                PhotoPath = string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(value.Vibe))
            {
                var vibeDisplay = value.Vibe.ToLowerInvariant().Replace("_", " ");
                if (vibeDisplay.Length > 0)
                    vibeDisplay = char.ToUpper(vibeDisplay[0]) + vibeDisplay[1..];
                var matchingOption = VibeOptions.FirstOrDefault(
                    o => o.Equals(vibeDisplay, StringComparison.OrdinalIgnoreCase));
                SelectedVibe = matchingOption ?? "None";
            }
            else
            {
                SelectedVibe = "None";
            }

            _isPhotoNew = false;
        }
        else
        {
            Content = string.Empty;
            PhotoPath = string.Empty;
            _existingPhotoUrl = null;
            _isPhotoNew = false;
            SelectedVibe = "None";
            SelectedVisibility = "Public";
        }

        OnPropertyChanged(nameof(PageTitle));
        OnPropertyChanged(nameof(IsEditing));
    }

    private async Task<string?> DownloadExistingPhotoAsync(string photoUrl)
    {
        if (string.IsNullOrWhiteSpace(photoUrl))
            return null;

        try
        {
            HttpClient client;
#if DEBUG
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (_, _, _, _) => true
            };
            client = new HttpClient(handler);
#else
            client = new HttpClient();
#endif
            var bytes = await client.GetByteArrayAsync(photoUrl);
            var localPath = Path.Combine(FileSystem.CacheDirectory, "preview_" + Guid.NewGuid() + ".jpg");
            await File.WriteAllBytesAsync(localPath, bytes);
            return localPath;
        }
        catch
        {
            return null;
        }
    }

    public List<string> VibeOptions { get; } = new()
    {
        "None",
        "Love",
        "Gratitude",
        "Apology",
        "Appreciation",
        "Mindfulness",
        "Thank You",
        "Joy",
        "Hope",
        "Comfort",
        "Sorrow"
    };
    public List<string> VisibilityOptions { get; } = new() { "Public", "Connections Only" };

    [ObservableProperty, NotifyPropertyChangedFor(nameof(VisibilityDisplay), nameof(VisibilityIcon))]
    private string _selectedVisibility = "Public";

    public bool IsVibeSelected => SelectedVibe != "None";
    public string VibePrefix => IsVibeSelected ? "✓ Current Vibe:" : "";
    public string VibeName => IsVibeSelected ? SelectedVibe : "❌ No vibe selected";


    [RelayCommand]
    private async Task SelectVibeAsync()
    {
        var options = VibeOptions
            .Select(v => v == SelectedVibe ? $"✓ {v}" : v)
            .ToArray();

        var result = await Shell.Current.DisplayActionSheet("Pick a Vibe", "Cancel", null, options);
        if (string.IsNullOrWhiteSpace(result) || result == "Cancel")
            return;
        var vibe = result.StartsWith("✓ ") ? result[2..] : result;
        SelectedVibe = vibe;
    }

    public string CurrentVibeDisplay =>
        SelectedVibe == "None" ? "No Vibe selected" : $"Current Vibe: ✓ {SelectedVibe}";

    private static string? GetVibeIcon(string? selectedVibe)
    {
        if (string.IsNullOrWhiteSpace(selectedVibe) || selectedVibe == "None")
            return null;

        var vibe = selectedVibe.ToLowerInvariant().Replace(" ", "_");
        return vibe switch
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
    }


    [ObservableProperty, NotifyPropertyChangedFor(nameof(CurrentVibeDisplay),
        nameof(IsVibeSelected), nameof(VibePrefix), nameof(VibeName))]
    private string _selectedVibe = "None";

    partial void OnSelectedVibeChanged(string value)
    {
        OnPropertyChanged(nameof(SelectedVibeIcon));
    }

    public string? SelectedVibeIcon => GetVibeIcon(SelectedVibe);
    public string VisibilityDisplay =>
        SelectedVisibility == "Connections Only"
            ? "Private Post (Connections Only)"
            : "Public Post";
    public string VisibilityIcon => SelectedVisibility == "Connections Only" ? "private_icon.svg" : "public_icon.svg";

    [RelayCommand]
    private async Task SelectVisibilityAsync()
    {
        var options = new[] { "Public", "Private" };
        var current = VisibilityDisplay;
        var markedOptions = options.Select(o => o == current ? $"✓ {o}" : o).ToArray();

        var result = await Shell.Current.DisplayActionSheet("Set visibility", "Cancel", null, markedOptions);
        if (string.IsNullOrWhiteSpace(result) || result == "Cancel")
            return;

        if (result.Contains("Public"))
            SelectedVisibility = "Public";
        else
            SelectedVisibility = "Connections Only";
    }
}