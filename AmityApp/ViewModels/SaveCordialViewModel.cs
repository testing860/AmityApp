using AmityApp.Apis;
using AmityApp.Shared.Dtos;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AmityApp.ViewModels;

public partial class SaveCordialViewModel : BaseViewModel
{

    private readonly ICordialsApi _cordialsApi;
    public SaveCordialViewModel(ICordialsApi cordialsApi)
    {
        _cordialsApi = cordialsApi;
    }

    [ObservableProperty]
    private string _content = string.Empty;

    [ObservableProperty]
    private string _photoPath = string.Empty;

    [RelayCommand]
    private async Task SelectPhotoAsync()
    {
        if (MediaPicker.Default.IsCaptureSupported)
        {
            const string PickFromDevice = "Pick from Device";
            const string CapturePhoto = "Capture Photo";

            var result = await Shell.Current.DisplayActionSheet("Choose photo", "Cancel", null, PickFromDevice, CapturePhoto);
            if (string.IsNullOrWhiteSpace(result))
                return;

            switch (result)
            {
                case PickFromDevice:
                    await PickFromDeviceAsync();
                    break;
                case CapturePhoto:
                    await CapturePhotoAsync();
                    break;
            }

            async Task PickFromDeviceAsync()
            {
                FileResult? fileResult = await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions
                {
                    Title = "Select Photo"
                });
                if (fileResult is null)
                {
                    await ToastAsync("No photo selected");
                    return;
                }
                PhotoPath = fileResult.FullPath;
            }

            async Task CapturePhotoAsync()
            {
                FileResult? fileResult = await MediaPicker.Default.CapturePhotoAsync(new MediaPickerOptions
                {
                    Title = "Take Photo"
                });
                if (fileResult is null)
                {
                    await ToastAsync("No photo captured");
                    return;
                }
                PhotoPath = fileResult.FullPath;
            }
        }
    }
    [RelayCommand]
    private void RemovePhoto()
    {
        PhotoPath = "";
    }

    [RelayCommand]
    private async Task SaveCordialAsync()
    {
        System.Diagnostics.Debug.WriteLine("🚀 SaveCordialAsync command FIRED!");
        if (string.IsNullOrWhiteSpace(Content) && string.IsNullOrWhiteSpace(PhotoPath))
        {
            await ToastAsync("Either content or photo is required");
            return;
        }


        await MakeApiCall(async () =>
        {
            StreamPart? photoStreamPart = null;
            if (!string.IsNullOrWhiteSpace(PhotoPath))
            {
                var fileName = Path.GetFileName(PhotoPath);
                var fileStream = File.OpenRead(PhotoPath);
                photoStreamPart = new StreamPart(fileStream, fileName);
            }
            var serializedSaveCordialDto = JsonSerializer.Serialize(new SaveCordialDto { Content = Content });

            var result = await _cordialsApi.SaveCordialAsync(photoStreamPart, serializedSaveCordialDto);
            if(!result.IsSuccess)
            {
                await ShowErrorAlertAsync(result.Error);
                return;
            }

            await ToastAsync("Cordial has been published successfully!");
            Content = null;
            PhotoPath = "";
            await NavigateBackAsync();
        });
    }
}
