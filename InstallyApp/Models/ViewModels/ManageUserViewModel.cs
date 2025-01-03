using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InstallyAPI.Models;
using InstallyApp.ViewModels;
using Microsoft.Maui.Controls;

namespace InstallyApp.Models.ViewModels;

[QueryProperty(nameof(User), "User")]
public partial class ManageUserViewModel : ObservableObject
{
    private readonly MainPageViewModel _mainPageViewModel;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(IsNew))]
    public UserEntity user = new();
    
    public ObservableCollection<UserEntity> Users { get; set; } = new();

    private bool IsNew
    {
        get
        {
            if (User.Guid == Guid.Empty) return true;
            return false;
        }
    }

    [RelayCommand]
    private async Task Save()
    {
        if (IsNew)
        {
            Debug.WriteLine("---> Add new Item");
            await App.DataService.AddUserAsync(User);
        }
        else
        {
            Debug.WriteLine("---> Update Item");
            await App.DataService.UpdateUserAsync(User);
        }
        
        /*
        MainPageViewModel.Users.Insert(0, User);
        */

        App.NavigationService.GoBack();
    }

    [RelayCommand]
    private async Task Delete()
    {
        await App.DataService.DeleteUserAsync(User.Guid);

        /*
        MainPageViewModel.Users.Remove(User);
        */

        App.NavigationService.GoBack();
    }

    [RelayCommand]
    private async Task Cancel()
    {
        App.NavigationService.GoBack();
    }
}