using AmityApp.Apis;
using AmityApp.Models;
using AmityApp.Pages;
using AmityApp.Services;
using AmityApp.Shared.Dtos;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmityApp.ViewModels;

public partial class ChimesViewModel : BaseCordialViewModel
{
    private readonly IUserApi _userApi;
    private readonly AuthService _authService;
    private readonly UpdatesService _updatesService;

    public ChimesViewModel(IUserApi userApi, AuthService authService, 
        UpdatesService updatesService, ICordialsApi cordialsApi) : base(cordialsApi)
    {
       _userApi = userApi;
       _authService = authService;
       _updatesService = updatesService;
    }

    public ObservableCollection<ChimeDto> Chimes { get; set; } = [];

    private int _startIndex = 0;
    private const int PageSize = 50;


    [RelayCommand]
    private async Task FetchChimesAsync()
    {
        await MakeApiCall(async () =>
        {
            var chimes = await _userApi.GetChimesAsync(_startIndex, PageSize);
            if (chimes.Length > 0)
            {
                if (_startIndex == 0 && Chimes.Count > 0)
                {
                    // Pull to refresh
                    Chimes.Clear();
                }
                _startIndex += chimes.Length;

                foreach (var chime in chimes)
                {
                    Chimes.Add(chime);
                }
            }
        });
    }

    [ObservableProperty]
    private bool _isRefreshing;


    [RelayCommand]
    private async Task RefreshChimesAsync()
    {
        _startIndex = 0;
        await FetchChimesAsync();
        IsRefreshing = false;
    }

    private async void OnChimeGenerated(ChimeDto dto)
    {

        if (dto.ForUserId == _authService.User.Id)
        {
            await Shell.Current.Dispatcher.DispatchAsync(()=> ToastAsync("New chime"));

            Chimes = [dto, .. Chimes];
            OnPropertyChanged(nameof(Chimes));
        }
    }

    public void ConfigureUpdates()
    {
        _updatesService.AddChimeGeneratedHandler(nameof(ChimesViewModel), OnChimeGenerated);
    }

    [RelayCommand]
    private async Task OpenCordialAsync(Guid? cordialId)
    {
        if (cordialId == null || cordialId == Guid.Empty)
        {
            await NavigateAsync(nameof(RequestsPage));
            return;
        }

        // Check if a Cordial‑related Chime actually belongs to a Connection Request
        var chime = Chimes.FirstOrDefault(c => c.CordialId == cordialId);
        if (chime != null && chime.Text.Contains("connect", StringComparison.OrdinalIgnoreCase))
        {
            await NavigateAsync(nameof(RequestsPage));
            return;
        }
        await MakeApiCall(async () =>
        {
            var cordial = await CordialsApi.GetCordialAsync(cordialId.Value);
            if (cordial == null)
            {
                await ToastAsync("Cordial no longer exists!");
                return;
            }
            GoToDetailsPageCommand.Execute(CordialModel.FromDto(cordial));
        });
    }
}
