using AmityApp.Services;
using CommunityToolkit.Maui.Alerts;
namespace AmityApp.Pages;

public partial class ProfilePage : ContentPage
{
    private readonly AuthService _authService;

    public ProfilePage(AuthService authService)
    {
        InitializeComponent();
        _authService = authService;
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        var username = _authService.User?.Name ?? "User";
        _authService.Logout();

        var toast = Toast.Make($"You are now logged out, {username}!");
        await toast.Show();
        await Shell.Current.GoToAsync($"//{nameof(LoginPage)}");
    }
}