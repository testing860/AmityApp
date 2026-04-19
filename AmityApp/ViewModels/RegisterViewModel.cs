using AmityApp.Apis;
using AmityApp.Pages;
using AmityApp.Shared.Dtos;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AmityApp.ViewModels;

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

            // Upload photo if provided

            await ToastAsync($"SUCCESS: You have just registered successfully, {(string.IsNullOrWhiteSpace(Name) ? "User" : Name)}!");
            await NavigateAsync($"//{nameof(LoginPage)}");
        });
    }
}
