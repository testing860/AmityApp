using AmityApp.Apis;
using AmityApp.Models;
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


[QueryProperty(nameof(Cordial), nameof(Cordial))]
public partial class DetailsViewModel : BaseCordialViewModel
{

    private readonly AuthService _authService;
    public DetailsViewModel(AuthService authService, ICordialsApi cordialsApi) : base(cordialsApi)
    {
        _authService = authService;
        SkipGoToDetailsCommandAction = true;
    }

    [ObservableProperty]
    private bool _isOwnCordial;

    [ObservableProperty]
    private CordialModel _cordial = new();

    public ObservableCollection<CommentDto> Comments { get; set; } = [];

    partial void OnCordialChanged(CordialModel value)
    {
        IsOwnCordial = value.UserId == _authService?.User?.Id;
        _ = FetchCommentsAsync();
    }

    private async Task FetchCommentsAsync()
    {
        await MakeApiCall(async () =>
        {
            var comments = await CordialsApi.GetCordialCommentsAsync(Cordial.CordialId, 0, 50);
            Comments.Clear();
            foreach (var comment in comments)
            {
                Comments.Add(comment);
            }
        });
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
            var newComment = result.Data;
            Comments = [newComment, .. Comments];
            OnPropertyChanged(nameof(Comments));
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
}
