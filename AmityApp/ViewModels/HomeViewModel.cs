using AmityApp.Apis;
using AmityApp.Models;
using AmityApp.Pages;
using AmityApp.Services;
using AmityApp.Shared.Dtos;
using AmityApp.Shared.Hubs;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmityApp.ViewModels;

public partial class HomeViewModel : BaseCordialViewModel
{
    private readonly UpdatesService _updatesService;
    private readonly AuthService _authService;

    public HomeViewModel(ICordialsApi cordialsApi, UpdatesService updatesService, AuthService authService) : base(cordialsApi)
    {
        FetchCordialsAsync();
        _updatesService = updatesService;
        _authService = authService;
    }

    public ObservableCollection<CordialModel> Cordials { get; set; } = [];

    private int _startIndex = 0;
    private const int PageSize = 6;

    [RelayCommand]
    private async Task FetchCordialsAsync()
    {
        await MakeApiCall(async () =>
        {
            CordialDto[] cordials;
            if (!ShowOnlyConnections)
                cordials = await CordialsApi.GetCordialsAsync(_startIndex, PageSize);
            else
                cordials = await CordialsApi.GetOnlyConnectionCordialsAsync(_startIndex, PageSize);

            if (cordials.Length > 0)
            {
                if (_startIndex == 0 && Cordials.Count > 0)
                    Cordials.Clear();

                _startIndex += cordials.Length;
                foreach (var c in cordials)
                    Cordials.Add(CordialModel.FromDto(c));
            }
        });
    }

    [ObservableProperty]
    private bool _isRefreshing;

    [RelayCommand]
    private async Task RefreshCordialsAsync()
    {
        _startIndex = 0;
        await FetchCordialsAsync();
        IsRefreshing = false;
    }

    [RelayCommand]
    private async Task GoToAddCordialAsync() =>
        await NavigateAsync (nameof(AddCordialPage));


    [ObservableProperty]
    private bool _IsThereNewChimes;


    private void OnCordialChanged(CordialDto cordial)
    {
        var currentCordial = Cordials.FirstOrDefault(c => c.CordialId == cordial.CordialId);
        if (currentCordial is not null)
        {
            currentCordial.PhotoUrl = cordial.PhotoUrl;
            currentCordial.Content = cordial.Content;
            currentCordial.Vibe = cordial.Vibe;
            currentCordial.PostedOnDisplay = cordial.PostedOnDisplay;
            if (cordial.Visibility == "ConnectionsOnly" && cordial.UserId != _authService.User.Id)
            {
                Cordials.Remove(currentCordial);
                _startIndex--;
            }
        }
    }

    private void OnCordialDeleted(Guid cordialId)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            var currentCordial = Cordials.FirstOrDefault(c => c.CordialId == cordialId);
            if (currentCordial != null)
            {
                Cordials.Remove(currentCordial);
                _startIndex--;
            }
        });
    }

    private void OnUserPhotoChanged(UserPhotoChangedDto dto)
    {
        foreach (var cordial in Cordials.Where(c => c.UserId == dto.UserId))
        {
            cordial.UserPhotoUrl = dto.PhotoUrl;
        }
    }

    private void OnChimeGenerated(ChimeDto dto)
    {
        if (dto.ForUserId == _authService.User.Id)
            IsThereNewChimes = true;
    }

    public void ConfigureUpdates()
    {
        _updatesService.AddCordialChangedHandler(nameof(HomeViewModel), OnCordialChanged);
        _updatesService.AddCordialDeletedHandler(nameof(HomeViewModel), OnCordialDeleted);
        _updatesService.AddChimeGeneratedHandler(nameof(HomeViewModel), OnChimeGenerated);
        _updatesService.AddUserPhotoChangedHandler(nameof(HomeViewModel), OnUserPhotoChanged);
    }

    public bool ShowOnlyConnections { get; set; } // default false

    public string FeedModeDisplay => ShowOnlyConnections ? "Connections Only" : "All Cordials";

    [RelayCommand]
    private async Task SelectFeedMode()
    {
        var options = new[] { "All Cordials", "Connections Only" };
        var current = ShowOnlyConnections ? "Connections Only" : "All Cordials";
        var markedOptions = options.Select(o => o == current ? $"✓ {o}" : o).ToArray();

        var result = await Shell.Current.DisplayActionSheet("Choose feed", "Cancel", null, markedOptions);
        if (string.IsNullOrWhiteSpace(result) || result == "Cancel")
            return;

        ShowOnlyConnections = result.Contains("Connections Only");
        OnPropertyChanged(nameof(FeedModeDisplay));
        _startIndex = 0;            // Reset Pagination
        await FetchCordialsAsync();
    }

    [RelayCommand]
    private async Task GoToRequestsAsync() => await NavigateAsync(nameof(RequestsPage));
}