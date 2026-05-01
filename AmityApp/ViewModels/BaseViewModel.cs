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

    protected static string FixLocalhostUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return url;

        if (Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
            uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase))
        {
            var builder = new UriBuilder(uri) { Host = "10.0.2.2" };
            return builder.Uri.ToString();
        }
        return url;
    }

    protected async Task<string?> ChoosePhotoAsync()
    {
        if (MediaPicker.Default.IsCaptureSupported)
        {
            const string PickFromDevice = "Pick from Device";
            const string CapturePhoto = "Capture Photo";

            var result = await Shell.Current.DisplayActionSheet("Choose photo", "Cancel", null, PickFromDevice, CapturePhoto);
            if (string.IsNullOrWhiteSpace(result))
                return null;

            switch (result)
            {
                case PickFromDevice:
                    return await PickFromDeviceAsync();
                case CapturePhoto:
                    return await CapturePhotoAsync();
                default:
                    return null;
            }

            async Task<string?> PickFromDeviceAsync()
            {
                var fileResult = await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions { Title = "Select Photo" });
                if (fileResult is null)
                {
                    await ToastAsync("No photo selected");
                    return null;
                }
                return fileResult.FullPath;
            }

            async Task<string?> CapturePhotoAsync()
            {
                var fileResult = await MediaPicker.Default.CapturePhotoAsync(new MediaPickerOptions { Title = "Take Photo" });
                if (fileResult is null)
                {
                    await ToastAsync("No photo captured");
                    return null;
                }
                return fileResult.FullPath;
            }
        }

        await ToastAsync("Capture is not supported :(");
        return null;
    }

    protected static string GetEmulatorImageUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return null;

        var result = url;
        result = result.Replace("https://localhost:7134", "http://10.0.2.2:5009")
                       .Replace("https://10.0.2.2:7134", "http://10.0.2.2:5009");
        if (!result.StartsWith("http://") && !result.StartsWith("https://"))
            result = $"http://10.0.2.2:5009/{result.Replace("\\", "/")}";
        return result;
    }


}