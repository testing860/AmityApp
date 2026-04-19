using AmityApp.Services;


namespace AmityApp.Pages;

public class InitPage : ContentPage
{

	public const string FirstLaunchKey = "first-launch";
	private readonly AuthService _authService;
	public InitPage(AuthService authService)
	{
		Content = new VerticalStackLayout
		{
			Children = {
				new Label { 
					HorizontalOptions = LayoutOptions.Center, 
					VerticalOptions = LayoutOptions.Center, 
					Text = "Initializing.."
				}
			}
		};
		_authService = authService;
	}

    protected override async void OnAppearing()
    {

        if (!Preferences.Default.ContainsKey(FirstLaunchKey))
        {
            await Shell.Current.GoToAsync($"//{nameof(OnBoardingPage)}");
            return;
        }

        if (_authService.IsLoggedIn)
        {
            await Shell.Current.GoToAsync($"//{nameof(HomePage)}");
            return;
        }
        await Shell.Current.GoToAsync($"//{nameof(LoginPage)}");
    }

}