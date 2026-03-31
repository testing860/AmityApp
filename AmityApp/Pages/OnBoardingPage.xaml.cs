using System.Threading.Tasks;

namespace AmityApp.Pages;

public partial class OnBoardingPage : ContentPage
{
	public OnBoardingPage()
	{
		InitializeComponent();
	}

	private async void Button_Clicked(object sender, EventArgs e)
	{
        await Shell.Current.GoToAsync($"//{nameof(LoginPage)}");
    }
}