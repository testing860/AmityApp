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


[QueryProperty(nameof(Cordial), nameof(Cordial))]
public partial class DetailsViewModel : BaseCordialViewModel
{

    private readonly AuthService _authService;
    private readonly UpdatesService _updatesService;

    public DetailsViewModel(AuthService authService, ICordialsApi cordialsApi, UpdatesService updatesService) : base(cordialsApi)
    {
        _authService = authService;
        _updatesService = updatesService;
        SkipGoToDetailsCommandAction = true;
        ConfigureUpdates();
    }

    [ObservableProperty]
    private bool _isOwnCordial;

    [ObservableProperty]
    private CordialModel _cordial = new();

    public ObservableCollection<CommentDto> Comments { get; set; } = [];

    async partial void OnCordialChanged(CordialModel value)
    {
        IsOwnCordial = value.UserId == _authService?.User?.Id;
        await FetchCommentsAsync();
    }


    private int _startIndex = 0;
    private const int PageSize = 10;

    [ObservableProperty]
    private bool _isRefreshingComments;

    [RelayCommand]
    private async Task FetchCommentsAsync()
    {
        await MakeApiCall(async () => {
            var comments = await CordialsApi.GetCordialCommentsAsync(Cordial.CordialId, _startIndex, PageSize);
            if (comments.Length > 0)
            {
                _startIndex += comments.Length;

                foreach (var c in comments)
                {
                    Comments.Add(c);
                }
            }
        });
    }

    [RelayCommand]
    private async Task RefreshCommentsAsync()
    {
        List<CommentDto> newComments = null;

        // Fetch Fresh Data
        await MakeApiCall(async () =>
        {
            newComments = (await CordialsApi.GetCordialCommentsAsync(
                Cordial.CordialId, 0, PageSize)).ToList();
        });

        // Only update the UI if the fetch was successful
        if (newComments != null)
        {
            Comments.Clear();
            foreach (var c in newComments)
                Comments.Add(c);

            _startIndex = newComments.Count;
        }

        IsRefreshingComments = false;
    }

    [ObservableProperty]
    private string? _comment;


    [RelayCommand]
    private async Task AddCommentAsync()
    {
        if (string.IsNullOrWhiteSpace(Comment))
        {
            await ToastAsync("Please enter comment");
            return;
        }

        await MakeApiCall(async () =>
        {
            var dto = new SaveCommentDto
            {
                CordialId = Cordial.CordialId,
                Content = Comment
            };
            var result = await CordialsApi.SaveCommentAsync(Cordial.CordialId, dto);
            if (!result.IsSuccess)
            {
                await ShowErrorAlertAsync(result.Error);
                return;
            }

            Comment = null;
        });
    }

    [RelayCommand]
    private async Task DeleteCordialAsync()
    {
        if (await Shell.Current.DisplayAlert("Confirm?", "Are you sure you want to delete this cordial?", "Yes", "No"))
        {
            await MakeApiCall(async () =>
            {
                var result = await CordialsApi.DeleteCordialAsync(Cordial.CordialId);
                if (!result.IsSuccess)
                {
                    await ShowErrorAlertAsync(result.Error);
                    return;
                }
                await ToastAsync("Cordial deleted");
                await NavigateAsync("..");
            });
        }
    }

    [RelayCommand]
    private async Task EditCordialAsync(CordialModel cordial)
    {
        var param = new Dictionary<string, object>
        {
            [nameof(SaveCordialViewModel.Cordial)] = cordial
        };
        await NavigateAsync(nameof(AddCordialPage), param);
    }

    private void OnCordialChanged(CordialDto cordial)
    {
        if(Cordial.CordialId ==cordial.CordialId)
        {
            Cordial.Content = cordial.Content;
            Cordial.PhotoUrl = cordial.PhotoUrl;
            Cordial.PostedOnDisplay = cordial.PostedOnDisplay;
            Cordial.NotifyFullPhotoUrlChanged();
        }
    }

    private void OnCordialDeleted(Guid cordialId)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            if (Cordial.CordialId == cordialId)
            {
                await ToastAsync("Cordial no longer exists");
                await NavigateBackAsync();
            }
        });
    }

    private void OnUserPhotoChanged(UserPhotoChangedDto dto)
    {
        if (Cordial.UserId == dto.UserId)
        {
            Cordial.UserPhotoUrl = dto.PhotoUrl;

            foreach (var comment in Comments.Where(c => c.UserId == dto.UserId))
            {
                comment.UserPhotoUrl = dto.PhotoUrl;
            }
        }
    }
    
    private void OnCommentAdded(CommentDto dto)
    {
        if (dto.CordialId == Cordial.CordialId)
        {
            Comments = [dto, .. Comments];
            OnPropertyChanged(nameof(Comments));
        }
    }

    public void ConfigureUpdates()
    {
        _updatesService.AddCordialChangedHandler(nameof(DetailsViewModel), OnCordialChanged);
        _updatesService.AddCordialDeletedHandler(nameof(DetailsViewModel), OnCordialDeleted);
        _updatesService.AddCommentAddedHandler(nameof(DetailsViewModel), OnCommentAdded);
        _updatesService.AddUserPhotoChangedHandler(nameof(DetailsViewModel), OnUserPhotoChanged);
    }
}
