using InstallyAPI.Models;
using InstallyApp.Models;
using InstallyApp.Models.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace InstallyApp.Pages;

public partial class ManageUsersPage : UserControl
{
    public ManageUsersPage()
    {
        InitializeComponent();
        
        DataContext = App.Services.GetRequiredService<ManageUserViewModel>();
    }
    
    public ManageUsersPage(Dictionary<string, object> navigationParameter)
    {
        var viewModel = App.Services.GetRequiredService<ManageUserViewModel>();
        viewModel.User = navigationParameter[nameof(UserEntity)] as UserEntity;
        DataContext = viewModel;
    }
}