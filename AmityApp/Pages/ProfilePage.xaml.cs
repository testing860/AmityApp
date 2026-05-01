using AmityApp.Services;
using AmityApp.ViewModels;

namespace AmityApp.Pages;

public partial class ProfilePage : ContentPage
{
    private readonly ProfileViewModel _viewModel;

    public ProfilePage(ProfileViewModel profileViewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = profileViewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (_viewModel.IsMyCordialsTabSelected && _viewModel.MyCordials.Count == 0)
            _viewModel.SelectMyCordialsTabCommand.Execute(null);
        else if (!_viewModel.IsMyCordialsTabSelected && _viewModel.MyCrownedCordials.Count == 0)
            _viewModel.SelectMyCrownedCordialsTabCommand.Execute(null);
    }
}