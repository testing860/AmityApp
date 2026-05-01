using AmityApp.Apis;
using AmityApp.Pages;
using AmityApp.Shared.Dtos;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Refit;

namespace AmityApp.ViewModels;

[QueryProperty(nameof(CroppedPhotoSource), "new-src")]
public partial class RegisterViewModel : BaseViewModel
{
    private readonly IAuthApi _authApi;
    public RegisterViewModel(IAuthApi authApi)
    {
        _authApi = authApi;
    }
    [ObservableProperty]
    private string _name;

    [ObservableProperty]
    private string _email;

    [ObservableProperty]
    private string _password;

    [ObservableProperty]
    private string _photoImageSource = "user.png";


    [RelayCommand]
    private async Task RegisterAsync()
    {
        if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            await Toast.Make("All fields are required").Show();
            return;
        }

        await MakeApiCall(async () => {
            var registerDto = new RegisterDto(Name, Email, Password);
            var result = await _authApi.RegisterAsync(registerDto);

            if (!result.IsSuccess)
            {
                await ShowErrorAlertAsync(result.Error);
                return;
            }

            var userId = result.Data;

            if(!string.IsNullOrWhiteSpace(PhotoImageSource) && PhotoImageSource != "user.png")
            {
                var photoName = Path.GetFileName(PhotoImageSource);
                using var fs = File.OpenRead(PhotoImageSource);

                var photoStreamPart = new StreamPart(fs, photoName);

                var apiResult = await _authApi.UploadPhotoAsync(userId, photoStreamPart);
                if (!result.IsSuccess)
                {
                    await ToastAsync("Photo upload failed");
                }
            }

            await ToastAsync($"SUCCESS: You have just registered successfully, {(string.IsNullOrWhiteSpace(Name) ? "User" : Name)}!");
            await NavigateAsync($"//{nameof(LoginPage)}");
        });
    }

    [RelayCommand]
    private async Task SelectPhotoAsync()
    {
        var selectedPhotoSource = await ChoosePhotoAsync();
        if (!string.IsNullOrWhiteSpace(selectedPhotoSource))
        {
            var param = new Dictionary<string, object>
            {
                [nameof(CropPhotoPage.PhotoSource)] = selectedPhotoSource
            };

            await NavigateAsync(nameof(CropPhotoPage), param);
        }
    }

    [ObservableProperty]
    private string? _croppedPhotoSource;

    async partial void OnCroppedPhotoSourceChanged(string? oldValue, string? newValue)
    {
        if (!string.IsNullOrWhiteSpace(newValue))
        {
            PhotoImageSource = newValue;
        }
    }
}
