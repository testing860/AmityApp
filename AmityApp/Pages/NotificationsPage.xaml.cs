namespace AmityApp.Pages;

public partial class NotificationsPage : ContentPage
{
    public NotificationsPage()
    {
        InitializeComponent();

        List<NotificationsModel> notifications = [
        new NotificationsModel(DateTime.Now, "This person lit your cordial"),
        new NotificationsModel(DateTime.Now.AddDays(-1), "This person commented on your cordial"),
        new NotificationsModel(DateTime.Now, "This person lit your cordial"),
        new NotificationsModel(DateTime.Now.AddMinutes(200), "This person lit your cordial"),
        new NotificationsModel(DateTime.Now.AddMonths(-5), "This person commented on your cordial"),
        new NotificationsModel(DateTime.Now, "This person wants to connect with you"),
        new NotificationsModel(DateTime.Now, "This person lit your cordial"),
        new NotificationsModel(DateTime.Now, "This person lit your cordial"),
        ];

        collection.ItemsSource = notifications;
    }
    public record NotificationsModel(DateTime On, string Text);
}