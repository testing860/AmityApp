using AmityApp.Apis;
using AmityApp.Handlers;
using AmityApp.Pages;
using AmityApp.Services;
using AmityApp.Shared;
using AmityApp.ViewModels;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Alerts;
using Microsoft.Extensions.Logging;
using Refit;
using Syncfusion.Maui.Core.Hosting;

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
                .UseMauiCommunityToolkit()
                .ConfigureSyncfusionCore();
#if DEBUG
            builder.Logging.AddDebug();
#endif
            builder.Services.AddSingleton<AuthService>();
            builder.Services.AddSingleton<UpdatesService>();

            builder.Services.AddSingleton<RegisterViewModel>()
                .AddTransient<RegisterPage>();
            builder.Services.AddSingleton<LoginViewModel>()
                .AddTransient<LoginPage>();
            builder.Services.AddSingleton<SaveCordialViewModel>()
                .AddTransient<AddCordialPage>();
            builder.Services.AddTransient<AuthHandler>();
            builder.Services.AddTransient<HomeViewModel>()
                .AddTransient<HomePage>();
            builder.Services.AddTransient<DetailsViewModel>()
                .AddTransient<CordialDetailsPage>();
            builder.Services.AddTransient<ProfileViewModel>()
                .AddTransient<ProfilePage>();
            builder.Services.AddTransient<ChimesViewModel>()
                 .AddTransient<NotificationsPage>();
            builder.Services.AddTransient<RequestsViewModel>()
                 .AddTransient<RequestsPage>();

            ConfigureRefit(builder.Services);

            var app = builder.Build();

            var authService = app.Services.GetRequiredService<AuthService>();
            authService.Initialize();

            return app;
        }

        private static void ConfigureRefit(IServiceCollection services)
        {
            services.AddRefitClient<IAuthApi>()
                    .ConfigureHttpClient(SetHttpClient)
                    .ConfigurePrimaryHttpMessageHandler(() =>
                        new HttpClientHandler
                        {
                            ServerCertificateCustomValidationCallback = (_, _, _, _) => true
                        });

            services.AddRefitClient<ICordialsApi>(GetRefitSettings)
                    .ConfigureHttpClient(SetHttpClient)
                    .AddHttpMessageHandler<AuthHandler>()
                    .ConfigurePrimaryHttpMessageHandler(() =>
                        new HttpClientHandler
                        {
                            ServerCertificateCustomValidationCallback = (_, _, _, _) => true
                        });

            services.AddRefitClient<IConnectionsApi>(GetRefitSettings)
                    .ConfigureHttpClient(SetHttpClient)
                    .AddHttpMessageHandler<AuthHandler>()
                    .ConfigurePrimaryHttpMessageHandler(() =>
                        new HttpClientHandler
                        {
                            ServerCertificateCustomValidationCallback = (_, _, _, _) => true
                        });

            services.AddRefitClient<IUserApi>(GetRefitSettings)
                    .ConfigureHttpClient(SetHttpClient)
                    .AddHttpMessageHandler<AuthHandler>()
                    .ConfigurePrimaryHttpMessageHandler(() =>
                        new HttpClientHandler
                        {
                            ServerCertificateCustomValidationCallback = (_, _, _, _) => true
                        });

            void SetHttpClient(HttpClient httpClient)
            {
                httpClient.BaseAddress = new Uri(AppConstants.ApiBaseUrl);
                httpClient.DefaultRequestHeaders.CacheControl =
                    new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };
            }

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