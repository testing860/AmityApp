using AmityApp.ViewModels;

namespace AmityApp.Pages;

public partial class AddCordialPage : ContentPage
{
	public AddCordialPage(SaveCordialViewModel saveCordialViewModel)
	{
		InitializeComponent();
		BindingContext = saveCordialViewModel;
	}
}