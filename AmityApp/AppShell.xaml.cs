using AmityApp.Pages;

namespace AmityApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            RegisterRoutes();
        }

        private static void RegisterRoutes()
        {
            Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
            Routing.RegisterRoute(nameof(CordialDetailsPage), typeof(CordialDetailsPage));
            Routing.RegisterRoute(nameof(AddCordialPage), typeof(AddCordialPage));
            Routing.RegisterRoute(nameof(ProfilePage), typeof(ProfilePage));
            Routing.RegisterRoute(nameof(NotificationsPage), typeof(NotificationsPage));
            Routing.RegisterRoute(nameof(CropPhotoPage), typeof(CropPhotoPage));
            Routing.RegisterRoute(nameof(RequestsPage), typeof(RequestsPage));
        }
    }
}
