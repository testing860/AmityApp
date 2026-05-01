using AmityApp.ViewModels;

namespace AmityApp.Pages;

public partial class RequestsPage : ContentPage
{
    public RequestsPage(RequestsViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is RequestsViewModel vm)
        {
            vm.SearchedUser = null;
            vm.FetchPendingRequestsCommand.Execute(null);
            vm.FetchAcceptedConnectionsCommand.Execute(null);
        }
    }
}