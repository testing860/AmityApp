using System.Threading.Tasks;

namespace AmityApp.Pages;

public partial class LoginPage : ContentPage
{
	public LoginPage()
	{
		InitializeComponent();
	}

    private async void LoginBtn_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"//{nameof(HomePage)}");
    }
    private async void RegisterBtn_Clicked(object sender, EventArgs e)
    {
		await Shell.Current.GoToAsync(nameof(RegisterPage));
    }
}