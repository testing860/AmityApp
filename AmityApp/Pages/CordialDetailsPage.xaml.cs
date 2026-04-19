using AmityApp.ViewModels;

namespace AmityApp.Pages;

public partial class CordialDetailsPage : ContentPage
{
	public CordialDetailsPage(DetailsViewModel detailsViewModel)
	{
		InitializeComponent();
        BindingContext = detailsViewModel;
	}

    private async void OnCloseTapped(object sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("..", animate: true); // Navigates back / closes modal
    }
}