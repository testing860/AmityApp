using AmityApp.Apis;
using AmityApp.Models;
using AmityApp.Pages;
using AmityApp.Shared.Dtos;
using CommunityToolkit.Mvvm.Input;

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

    [RelayCommand]
    private async Task ToggleCandleAsync(CordialModel cordial)
    {
        await MakeApiCall(async () => {
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

    [RelayCommand]
    private async Task ToggleCrownAsync(CordialModel cordial)
    {
        await MakeApiCall(async () => {
            var originalStatus = cordial.IsCrowned;
            cordial.IsCrowned = !cordial.IsCrowned;

            var result = await CordialsApi.ToggleCrownAsync(cordial.CordialId);
            if (!result.IsSuccess)
            {
                await ShowErrorAlertAsync(result.Error);
                cordial.IsCrowned = originalStatus;
                return;
            }

            var message = cordial.IsCrowned ? "Cordial crowned 👑" : "Cordial uncrowned!";
            await ToastAsync(message);
        });
    }
}