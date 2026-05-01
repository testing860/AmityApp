using AmityApp.Services;
using AmityApp.ViewModels;
using System.Threading.Tasks;

namespace AmityApp.Pages
{
    public partial class HomePage : ContentPage
    {
        private readonly HomeViewModel _homeViewModel;
        private readonly UpdatesService _updatesService;

        public HomePage(HomeViewModel homeViewModel, UpdatesService updatesService)
        {
            InitializeComponent();
            BindingContext = homeViewModel;
            _homeViewModel = homeViewModel;
            _updatesService = updatesService;
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
            _homeViewModel.ConfigureUpdates();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _updatesService.RemoveHandlers(nameof(HomeViewModel));
        }

        private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(CordialDetailsPage), animate: true);
        }

        private async void GoToProfile_Tapped(object sender, TappedEventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(ProfilePage), animate: true);
        }

        private async void GoToChimes_Tapped(object sender, TappedEventArgs e)
        {
            if (BindingContext is HomeViewModel vm)
                vm.IsThereNewChimes = false;
            await Shell.Current.GoToAsync(nameof(NotificationsPage), animate: true);
        }
    }
}
