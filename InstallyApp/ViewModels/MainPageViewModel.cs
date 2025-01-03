using System.Collections.ObjectModel;
using InstallyAPI.Models;
using InstallyApp.Models;

namespace InstallyApp.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        public static ObservableCollection<UserEntity> Users { get; set; } = new();
    }
}