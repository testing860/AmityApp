using AmityApp.Services;
using AmityApp.ViewModels;

namespace AmityApp.Pages;

public partial class NotificationsPage : ContentPage
{
    private readonly ChimesViewModel _chimesViewModel;
    private readonly UpdatesService _updatesService;

    public NotificationsPage(ChimesViewModel chimesViewModel, UpdatesService updatesService)
    {
        InitializeComponent();
        _chimesViewModel = chimesViewModel;
        _updatesService = updatesService;
        BindingContext = chimesViewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _chimesViewModel.ConfigureUpdates();
        if (_chimesViewModel.Chimes.Count == 0)
            _chimesViewModel.FetchChimesCommand.Execute(null);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _updatesService.RemoveHandlers(nameof(ChimesViewModel));
    }
}