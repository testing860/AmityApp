using AmityApp.Apis;
using AmityApp.Models;
using AmityApp.Pages;
using AmityApp.Services;
using AmityApp.Shared.Dtos;
using AmityApp.Shared.Hubs;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Refit;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmityApp.ViewModels;

[QueryProperty(nameof(CroppedPhotoSource), "new-src")]
public partial class ProfileViewModel : BaseCordialViewModel
{

    private readonly AuthService _authService;
    private readonly IUserApi _userApi;
    private readonly UpdatesService _updatesService;
    private bool _isFetchingMyCrownedCordials;
    private bool _allCrownedCordialsLoaded;

    public ProfileViewModel(ICordialsApi cordialsApi, AuthService authService, IUserApi userapi, UpdatesService updatesService) : base(cordialsApi)
    {
        User = authService.User!;
        _authService = authService;
        _userApi = userapi;
        _updatesService = updatesService;
        FetchMyCordialsAsync();
    }

    [ObservableProperty, NotifyPropertyChangedFor(nameof(UserPhotoDisplayUrl))]
    private LoggedInUser _user;

    [RelayCommand]
    public async Task LogoutAsync()
    {
        if(await Shell.Current.DisplayAlert("Confirm Logout?", "Do you really want to log out?", "Yes", "No"))
        {
            _authService.Logout();
            await NavigateAsync($"//{nameof(LoginPage)}");
        }
    }

    [ObservableProperty, NotifyPropertyChangedFor(nameof(IsCrownsTabSelected))]
    private bool _isMyCordialsTabSelected = true;
    public bool IsCrownsTabSelected => !IsMyCordialsTabSelected;

    private int _myCordialsStartIndex = 0;
    public ObservableCollection<CordialModel> MyCordials { get; set; } = [];

    private int _myCrownedCordialsStartIndex = 0;
    public ObservableCollection<CordialModel> MyCrownedCordials { get; set; } = [];

    private const int PageSize = 4;

    [RelayCommand]
    private async Task SelectMyCordialsTabAsync()
    {
        IsMyCordialsTabSelected = true;
        _myCordialsStartIndex = 0;
        await FetchMyCordialsAsync();
    }

    [RelayCommand]
    private async Task SelectMyCrownedCordialsTabAsync()
    {
        IsMyCordialsTabSelected = false;
        _myCrownedCordialsStartIndex = 0;
        _allCrownedCordialsLoaded = false;
        MyCrownedCordials.Clear();
        await FetchMyCrownedCordialsAsync();
    }


    [RelayCommand]
    private async Task FetchMyCordialsAsync()
    {
        await MakeApiCall(async () =>
        {
            var cordials = await _userApi.GetUserCordialsAsync(_myCordialsStartIndex, PageSize);
            if (cordials.Length > 0)
                
                if (_myCordialsStartIndex == 0 && MyCordials.Count > 0 )
                {
                    MyCordials.Clear();
                }

                _myCordialsStartIndex += cordials.Length;

            foreach (var c in cordials)
            {
                MyCordials.Add(CordialModel.FromDto(c));
            }
        });
    }

    [RelayCommand]
    private async Task FetchMyCrownedCordialsAsync()
    {
        if (_isFetchingMyCrownedCordials || _allCrownedCordialsLoaded)
            return;

        _isFetchingMyCrownedCordials = true;

        await MakeApiCall(async () =>
        {
            var cordials = await _userApi.GetUserCrownedCordialsAsync(_myCrownedCordialsStartIndex, PageSize);
            if (cordials.Length > 0)
            {
                _myCrownedCordialsStartIndex += cordials.Length;
                foreach (var c in cordials)
                    MyCrownedCordials.Add(CordialModel.FromDto(c));

                if (cordials.Length < PageSize)
                    _allCrownedCordialsLoaded = true;
            }
            else
            {
                _allCrownedCordialsLoaded = true;
            }
        });

        _isFetchingMyCrownedCordials = false;
    }

    [RelayCommand]
    private async Task CreateCordialAsync() => await NavigateAsync(nameof(AddCordialPage));

    [RelayCommand]
    private async Task GoToHomeAsync() => await NavigateAsync($"//{nameof(HomePage)}");


    [RelayCommand]
    private async Task ChangePhotoAsync()
    {
        var selectedPhotoSource = await ChoosePhotoAsync();
        if (!string.IsNullOrWhiteSpace(selectedPhotoSource))
        {
            var param = new Dictionary<string, object>
            {
                [nameof(CropPhotoPage.PhotoSource)] = selectedPhotoSource
            };

            await NavigateAsync(nameof(CropPhotoPage), param);
        }
    }

    [ObservableProperty]
    private string? _croppedPhotoSource;

    async partial void OnCroppedPhotoSourceChanged(string? oldValue, string? newValue)
    {
        if (!string.IsNullOrWhiteSpace(newValue))
        {
            await MakeApiCall(async () => {
                var photoName = Path.GetFileName(newValue);
                using var fs = File.OpenRead(newValue);

                var photoStreamPart = new StreamPart(fs, photoName);

                var result = await _userApi.ChangePhotoAsync(photoStreamPart);
                if (!result.IsSuccess)
                {
                    await ShowErrorAlertAsync(result.Error);
                    return;
                }

                var newPhotoUrl = result.Data;

                User = User with { PhotoUrl = newPhotoUrl };
                _authService.Login(new LoginResponseDto(User, _authService.Token));

                foreach (var cordial in MyCordials)
                {
                    cordial.UserPhotoUrl = newPhotoUrl;
                }
            });
        }
    }

    public string? UserPhotoDisplayUrl
    {
        get
        {
            if (string.IsNullOrWhiteSpace(User?.PhotoUrl))
                return "user.png";

            var url = User.PhotoUrl;

            // DEVELOPMENT HTTPS with HTTP emulator URL (same as CordialModel)
            url = url.Replace("https://localhost:7134", "http://10.0.2.2:5009")
                     .Replace("https://10.0.2.2:7134", "http://10.0.2.2:5009");

            // If still a relative path, prepend base URL
            if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                url = $"http://10.0.2.2:5009/{url.Replace("\\", "/")}";
            }

            return url;
        }
    }

    [RelayCommand]
    private async Task GoToRequestsAsync() => await NavigateAsync(nameof(RequestsPage));

    protected override void OnToggleCrownAsync(CordialModel cordial)
    {
        var currentCordial = MyCrownedCordials.FirstOrDefault(c => c.CordialId == cordial.CordialId);
        if ((currentCordial != null) && !cordial.IsCrowned)
        {
            MyCrownedCordials.Remove(currentCordial);
        }
    }
}