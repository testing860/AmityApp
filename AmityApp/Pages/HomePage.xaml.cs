using AmityApp.ViewModels;
using System.Threading.Tasks;

namespace AmityApp.Pages
{
    public partial class HomePage : ContentPage
    {

        public HomePage(HomeViewModel homeViewModel)
        {
            InitializeComponent();
            BindingContext = homeViewModel;
        }

        private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(CordialDetailsPage), animate: true);
        }

        private async void GoToProfile_Tapped(object sender, TappedEventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(ProfilePage), animate: true);
        }

        private async void GoToNotifications_Tapped(object sender, TappedEventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(NotificationsPage), animate: true);
        }
    }
}
