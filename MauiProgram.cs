using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;

namespace AmityApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("PlusJakartaSans-Regular.ttf", "PlusJakartaSansRegular");
                    fonts.AddFont("PlusJakartaSans-Bold.ttf", "PlusJakartaSansBold");

                })
                .UseMauiCommunityToolkit();
#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
