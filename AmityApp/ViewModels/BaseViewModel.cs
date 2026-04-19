using AmityApp.Apis;
using AmityApp.Pages;
using AmityApp.Services;
using AmityApp.Shared.Dtos;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmityApp.ViewModels;

public partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isBusy;

    protected async Task ShowErrorAlertAsync(string message) =>
    await Shell.Current.DisplayAlert("Error", message, "Ok");

    protected async Task NavigateAsync(string url) =>
        await Shell.Current.GoToAsync(url, animate: true);

    protected async Task NavigateAsync(string url, Dictionary<string, object> parameters) =>
    await Shell.Current.GoToAsync(url, animate: true, parameters);

    protected async Task NavigateBackAsync() => await NavigateAsync("..");

    protected async Task ToastAsync(string message) =>
        await Toast.Make(message).Show();

    protected async Task MakeApiCall(Func<Task> apiCall)
    {
        IsBusy = true;
        try
        {
            await apiCall.Invoke();
        }
        catch (ApiException ex)
        {
            if(ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {

                if (await Shell.Current.DisplayAlert("Login Expired", "You have been automatically logged out. Do you want to log back in?", 
                    "Yes, Go to Login!",
                    "No, keep me here"))
                {
                    await NavigateAsync($"//{nameof(LoginPage)}");
                }
            }
            else
            {
                await ShowErrorAlertAsync(ex.Message);
            }
        }

        catch (Exception ex)
        {
            await ShowErrorAlertAsync(ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }




}