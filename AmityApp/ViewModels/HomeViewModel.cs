using AmityApp.Apis;
using AmityApp.Models;
using AmityApp.Pages;
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

public partial class HomeViewModel : BaseCordialViewModel
{

    public HomeViewModel(ICordialsApi cordialsApi) : base(cordialsApi)
    {
        FetchCordialsAsync();
    }

    public ObservableCollection<CordialModel> Cordials { get; set; } = [];

    private int _startIndex = 0;
    private const int PageSize = 6;

    [RelayCommand]
    private async Task FetchCordialsAsync()
    {
        await MakeApiCall(async () =>
        {
        var cordials = await CordialsApi.GetCordialsAsync(_startIndex, PageSize);
        if (cordials.Length > 0)
        {
            if (_startIndex == 0 && Cordials.Count > 0)
            {
                    Cordials.Clear();
                }

                _startIndex += cordials.Length;

                foreach (var c in cordials)
                {
                    Cordials.Add(CordialModel.FromDto(c));
                }
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
}