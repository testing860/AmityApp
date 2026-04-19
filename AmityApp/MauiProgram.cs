using AmityApp.Apis;
using AmityApp.Handlers;
using AmityApp.Pages;
using AmityApp.Services;
using AmityApp.ViewModels;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Alerts;
using Microsoft.Extensions.Logging;
using Refit;

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
            builder.Services.AddSingleton<AuthService>();
            builder.Services.AddSingleton<RegisterViewModel>()
                .AddTransient<RegisterPage>();
            builder.Services.AddSingleton<LoginViewModel>()
                .AddTransient<LoginPage>();
            builder.Services.AddSingleton<SaveCordialViewModel>()
                .AddTransient<AddCordialPage>();
            builder.Services.AddTransient<AuthHandler>();
            builder.Services.AddSingleton<HomeViewModel>()
                .AddSingleton<HomePage>();
            builder.Services.AddTransient<DetailsViewModel>()
                .AddTransient<CordialDetailsPage>();

            ConfigureRefit(builder.Services);

            var app = builder.Build();

            var authService = app.Services.GetRequiredService<AuthService>();
            authService.Initialize();

            return app;
        }

        private static void ConfigureRefit(IServiceCollection services)
        {
            var baseApiUrl = "https://10.0.2.2:7134";
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (_, _, _, _) => true
            };

            services.AddRefitClient<IAuthApi>()
                    .ConfigureHttpClient(SetHttpClient)
                    .ConfigurePrimaryHttpMessageHandler(() => handler);

            services.AddRefitClient<ICordialsApi>(GetRefitSettings)
                    .ConfigureHttpClient(SetHttpClient)
                    .AddHttpMessageHandler<AuthHandler>()
                    .ConfigurePrimaryHttpMessageHandler(() => handler);

            services.AddRefitClient<IUserApi>(GetRefitSettings)
                    .ConfigureHttpClient(SetHttpClient)
                    .AddHttpMessageHandler<AuthHandler>()
                    .ConfigurePrimaryHttpMessageHandler(() => handler);

            void SetHttpClient(HttpClient httpClient) => httpClient.BaseAddress = new Uri(baseApiUrl);

            RefitSettings GetRefitSettings(IServiceProvider sp)
            {
                var authService = sp.GetRequiredService<AuthService>();
                return new RefitSettings
                {
                    AuthorizationHeaderValueGetter = (_, _) =>
                    {
                        var token = authService.Token ?? "";
                        return Task.FromResult(token);
                    }
                };
            }
        }
    }
}