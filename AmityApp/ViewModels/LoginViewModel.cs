using AmityApp.Apis;
using AmityApp.Pages;
using AmityApp.Services;
using AmityApp.Shared.Dtos;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AmityApp.ViewModels;

public partial class LoginViewModel : BaseViewModel
{
    private readonly IAuthApi _authApi;
    private readonly AuthService _authService;
    public LoginViewModel(IAuthApi authApi, AuthService authService)
    {
        _authApi = authApi;
        _authService = authService;
    }

    [ObservableProperty]
    private string _email;

    [ObservableProperty]
    private string _password;


    [RelayCommand]
    private async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            await Toast.Make("All fields are required").Show();
            return;
        }

        await MakeApiCall(async () => {
            var loginDto = new LoginDto(Email, Password);
            var result = await _authApi.LoginAsync(loginDto);

            if (!result.IsSuccess)
            {
                await ShowErrorAlertAsync(result.Error);
                return;
            }

            LoginResponseDto loginResponse = result.Data;

            _authService.Login(loginResponse);

            await ToastAsync($"You are now logged in, {loginResponse.User?.Name ?? "User"}!");
            await NavigateAsync($"//{nameof(HomePage)}");
        });
    }
}