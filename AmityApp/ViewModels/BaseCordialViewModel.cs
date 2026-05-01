using AmityApp.Apis;
using AmityApp.Models;
using AmityApp.Pages;
using AmityApp.Shared.Dtos;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.ApplicationModel.DataTransfer;

namespace AmityApp.ViewModels;

public partial class BaseCordialViewModel : BaseViewModel 
{
    public BaseCordialViewModel(ICordialsApi cordialsApi)
    {
        CordialsApi = cordialsApi;
    }

    public ICordialsApi CordialsApi { get; }

    protected bool SkipGoToDetailsCommandAction { get; set; }

    [RelayCommand]
    private async Task GoToDetailsPageAsync(CordialModel cordial)
    {
        if (SkipGoToDetailsCommandAction)
            return;
        var param = new Dictionary<string, object>
        {
            [nameof(DetailsViewModel.Cordial)] = cordial
        };
        await NavigateAsync(nameof(CordialDetailsPage), param);
    }



    protected virtual void OnToggleCrownAsync(CordialModel cordial)
    {

    }

    [RelayCommand]
    private async Task ToggleCandleAsync(CordialModel cordial)
    {
        await MakeApiCall(async () =>
        {
            var originalStatus = cordial.IsLit;
            cordial.IsLit = !cordial.IsLit;

            var result = await CordialsApi.ToggleCandleAsync(cordial.CordialId);
            if (!result.IsSuccess)
            {
                await ShowErrorAlertAsync(result.Error);
                cordial.IsLit = originalStatus;
                return;
            }

            var message = cordial.IsLit ? "Cordial lit 🔥" : "Candle extinguished!";
            await ToastAsync(message);
        });
    }


    // 1: Automatically creates ToggleCrownCommand for XAML binding (MAUI inherent feature)
    [RelayCommand]

    // 2: Asynchronous, Private method that receives the selected cordial from the UI
    private async Task ToggleCrownAsync(CordialModel cordial)
    {
        System.Diagnostics.Debug.WriteLine($"[CROWN] Toggling CordialId = '{cordial.CordialId}'");

        // 3: Custom helper (defined in BaseViewModel) -> manages loading spinner and global errors
        await MakeApiCall(async () =>
        {

            // 4: Stores the current bookmark state before changing it.
            var originalStatus = cordial.IsCrowned;

            // 5: Optimistic UI update -> toggles the icon immediately (assumes success).
            cordial.IsCrowned = !cordial.IsCrowned;

            // 6: Calls the actual backend API and waits for the response.
            var result = await CordialsApi.ToggleCrownAsync(cordial.CordialId);

            // 7: Checks if the server returned an error.
            if (!result.IsSuccess)
            {
                // 8: Custom helper (defined in BaseViewModel) -> Shows an error if it did not succeed.
                await ShowErrorAlertAsync(result.Error);

                // 9: Reverts the UI change because the operation had not succeeded.
                cordial.IsCrowned = originalStatus;

                // 10: Exits the method early without displaying a success message
                return;
            }

            // 11: Chooses the appropriate success text using a ternary operator (
            var message = cordial.IsCrowned ? "Cordial crowned 👑" : "Cordial uncrowned!";

            // 12: Custom helper (defined in BaseViewModel) –> shows a temporary confirmation pop‑up.
            await ToastAsync(message);
            OnToggleCrownAsync(cordial);
        });
    }

    [RelayCommand]
    private async Task ShareCordialAsync(CordialModel cordial)
    {
        if (string.IsNullOrWhiteSpace(cordial.PhotoUrl))
        {
            // Text-only sharing (correct API)
            await Share.Default.RequestAsync(new ShareTextRequest
            {
                Text = cordial.Content,
                Title = "Amity"
            });
        }
        else
        {
            // Convert 'localhost' to the emulator's loopback address
            string photoUrl = FixLocalhostUrl(cordial.PhotoUrl);
            var localPath = await DownloadPhotoAsync(photoUrl);

            if (!string.IsNullOrWhiteSpace(localPath))
            {
                await Share.Default.RequestAsync(new ShareFileRequest
                {
                    Title = "Amity",
                    File = new ShareFile(localPath)
                });
            }
        }
    }

    private async Task<string?> DownloadPhotoAsync(string photoUrl)
    {
        try
        {
            // Allow Self-Signed Certs (DEBUG MODE)
            HttpClient client;
#if DEBUG
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (_, _, _, _) => true
            };
            client = new HttpClient(handler);
#else
        client = new HttpClient();
#endif

            var bytes = await client.GetByteArrayAsync(photoUrl);

            var filePath = Path.Combine(FileSystem.CacheDirectory, Guid.NewGuid() + ".jpg");
            await File.WriteAllBytesAsync(filePath, bytes);

            return filePath;
        }
        catch (Exception ex)
        {
            await ShowErrorAlertAsync(ex.Message);
            return null;
        }
    }

}