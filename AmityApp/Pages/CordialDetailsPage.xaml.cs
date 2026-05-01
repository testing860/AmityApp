using AmityApp.Services;
using AmityApp.ViewModels;

namespace AmityApp.Pages;

public partial class CordialDetailsPage : ContentPage
{
    private readonly DetailsViewModel _detailsViewModel;
    private readonly UpdatesService _updatesService;

    public CordialDetailsPage(DetailsViewModel detailsViewModel, UpdatesService updatesService)
	{
		InitializeComponent();
        BindingContext = detailsViewModel;
        _detailsViewModel = detailsViewModel;
        _updatesService = updatesService;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _detailsViewModel.ConfigureUpdates();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _updatesService.RemoveHandlers(nameof(DetailsViewModel));
    }
}