using AmityApp.Apis;
using AmityApp.Services;
using AmityApp.Shared.Dtos;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace AmityApp.ViewModels;

public partial class RequestsViewModel : BaseViewModel
{
    private readonly IConnectionsApi _connectionsApi;
    private readonly IUserApi _userApi;
    private readonly AuthService _authService;

    public RequestsViewModel(IConnectionsApi connectionsApi, IUserApi userApi, AuthService authService)
    {
        _connectionsApi = connectionsApi;
        _userApi = userApi;
        _authService = authService;
    }

    public ObservableCollection<ConnectionDto> PendingRequests { get; } = new();
    public ObservableCollection<ConnectionDto> AcceptedConnections { get; } = new();

    [ObservableProperty]
    private string _searchEmail = string.Empty;

    [ObservableProperty]
    private UserDto? _searchedUser;

    // ───────────── FETCH COMMANDS ─────────────
    [RelayCommand]
    private async Task FetchPendingRequests()
    {
        await MakeApiCall(async () =>
        {
            var list = await _connectionsApi.GetPendingRequests();
            PendingRequests.Clear();
            foreach (var item in list)
                PendingRequests.Add(item);
        });
    }

    [RelayCommand]
    private async Task FetchAcceptedConnections()
    {
        await MakeApiCall(async () =>
        {
            var list = await _connectionsApi.GetAcceptedConnections();
            if (list != null && list.Count > 0)
            {
                AcceptedConnections.Clear();
                foreach (var item in list)
                    AcceptedConnections.Add(item);
            }
        });
    }

    // ───────────── SEARCH ─────────────
    [RelayCommand]
    private async Task SearchUserAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchEmail))
        {
            await ToastAsync("Please enter an email address");
            return;
        }
        await MakeApiCall(async () =>
        {
            var result = await _userApi.SearchUserAsync(SearchEmail.Trim());
            if (!result.IsSuccess)
            {
                await ShowErrorAlertAsync(result.Error);
                SearchedUser = null;
            }
            else
            {
                SearchedUser = result.Data;
                if (SearchedUser == null)
                    await ToastAsync("User not found.");
            }
        });
    }

    [RelayCommand]
    private async Task SendRequestToSearchedUserAsync()
    {
        if (SearchedUser == null) return;
        await MakeApiCall(async () =>
        {
            var result = await _connectionsApi.SendRequest(SearchedUser.Id);
            if (result.IsSuccess)
                await ToastAsync("Connection request sent!");
            else
                await ShowErrorAlertAsync(result.Error);
        });
    }

    // ───────────── ACCEPT, REJECT & DELETE ─────────────
    [RelayCommand]
    private async Task AcceptRequest(ConnectionDto request)
    {
        await MakeApiCall(async () =>
        {
            var result = await _connectionsApi.AcceptRequest(request.ConnectionId);
            if (result.IsSuccess)
            {
                await ToastAsync("Connection accepted!");
                PendingRequests.Remove(request);
                AcceptedConnections.Add(request);

                await FetchAcceptedConnections();
            }
            else
                await ShowErrorAlertAsync(result.Error);
        });
    }

    [RelayCommand]
    private async Task RejectRequest(ConnectionDto request)
    {
        bool confirm = await Shell.Current.DisplayAlert(
            "Decline?",
            "Do you really want to decline this request?",
            "Yes", "No");
        if (!confirm) return;

        await MakeApiCall(async () =>
        {
            var result = await _connectionsApi.RejectRequest(request.ConnectionId);
            if (result.IsSuccess)
            {
                await ToastAsync("Request declined.");
                PendingRequests.Remove(request);
            }
            else
                await ShowErrorAlertAsync(result.Error);
        });
    }

    [RelayCommand]
    private async Task DeleteConnection(ConnectionDto connection)
    {
        bool confirm = await Shell.Current.DisplayAlert(
            "Unconnect?",
            "Do you really want to unconnect from this user?",
            "Yes", "No");
        if (!confirm) return;

        await MakeApiCall(async () =>
        {
            var result = await _connectionsApi.DeleteConnection(connection.ConnectionId);
            if (result.IsSuccess)
            {
                await ToastAsync("Connection removed.");
                AcceptedConnections.Remove(connection);
            }
            else
                await ShowErrorAlertAsync(result.Error);
        });
    }
}