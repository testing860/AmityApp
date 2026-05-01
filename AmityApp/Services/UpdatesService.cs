using AmityApp.Shared;
using AmityApp.Shared.Dtos;
using AmityApp.Shared.Hubs;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmityApp.Services;

public class UpdatesService
{
    public UpdatesService()
    {
        ConfigureRealtimeUpdates();
    }

    private readonly Dictionary<string, Action<CordialDto>> _cordialChangedActions = [];
    public void AddCordialChangedHandler(string key, Action<CordialDto> handler) =>
        _cordialChangedActions[key] = handler;

    private readonly Dictionary<string, Action<Guid>> _cordialDeletedActions = [];
    public void AddCordialDeletedHandler(string key, Action<Guid> handler) =>
        _cordialDeletedActions[key] = handler;

    private readonly Dictionary<string, Action<CommentDto>> _commentAddedActions = [];
    public void AddCommentAddedHandler(string key, Action<CommentDto> handler) =>
        _commentAddedActions[key] = handler;

    private readonly Dictionary<string, Action<UserPhotoChangedDto>> _userPhotoChangedActions = [];
    public void AddUserPhotoChangedHandler(string key, Action<UserPhotoChangedDto> handler) =>
        _userPhotoChangedActions[key] = handler;

    private readonly Dictionary<string, Action<ChimeDto>> _chimeGeneratedActions = [];
    public void AddChimeGeneratedHandler(string key, Action<ChimeDto> handler) =>
        _chimeGeneratedActions[key] = handler;

    private async Task ConfigureRealtimeUpdates()
    {
        try
        {
            var hubConnection = new HubConnectionBuilder()
                .WithUrl(AppConstants.HubFullUrl)
                .Build();

            // Configure Methods
            hubConnection.On<CordialDto>(nameof(INotificationsHubClient.CordialChanged), cordial =>
            {
                foreach (var (key, action) in _cordialChangedActions)
                {
                    try
                    {
                        action.Invoke(cordial);
                    }
                    catch (Exception ex)
                    {
                        // Log exception
                    }
                }
            });
            hubConnection.On<Guid>(nameof(INotificationsHubClient.CordialDeleted), cordialId =>
            {
                foreach (var (key, action) in _cordialDeletedActions)
                {
                    try
                    {
                        action.Invoke(cordialId);
                    }
                    catch (Exception ex)
                    {
                        // Log Exception
                    }
                }
            });
            hubConnection.On<CommentDto>(nameof(INotificationsHubClient.CommentAddedToCordial), comment
                =>
            {
                foreach (var (key, action) in _commentAddedActions)
                {
                    try
                    {
                        action.Invoke(comment);
                    }
                    catch (Exception ex)
                    {
                        // Log exception
                    }
                }
            });
            hubConnection.On<UserPhotoChangedDto>(nameof(INotificationsHubClient.UserPhotoChanged), UserPhotoChangedDto =>
            {
                foreach (var (key, action) in _userPhotoChangedActions)
                {
                    try
                    {
                        action.Invoke(UserPhotoChangedDto);
                    }
                    catch (Exception ex)
                    {
                        // Log Exception
                    }
                }
            });
            hubConnection.On<ChimeDto>(nameof(INotificationsHubClient.ChimeGenerated), chimeDto =>
            {
                foreach (var (key, action) in _chimeGeneratedActions)
                {
                    try
                    {
                        action.Invoke(chimeDto);
                    }
                    catch (Exception ex)
                    {
                        // Log Exception
                    }
                }
            });

            await hubConnection.StartAsync();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", "Real-time notification update failed!", "Okay");
        }
    }

    public void RemoveHandlers(string key)
    {
        if (_cordialChangedActions.ContainsKey(key))
            _cordialChangedActions.Remove(key);

        if (_cordialDeletedActions.ContainsKey(key))
            _cordialDeletedActions.Remove(key);

        if (_commentAddedActions.ContainsKey(key))
            _commentAddedActions.Remove(key);

        if (_userPhotoChangedActions.ContainsKey(key))
            _userPhotoChangedActions.Remove(key);

        if (_chimeGeneratedActions.ContainsKey(key))
            _chimeGeneratedActions.Remove(key);
    }
}
