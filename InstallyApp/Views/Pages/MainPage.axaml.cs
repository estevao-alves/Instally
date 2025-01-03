using InstallyAPI.Models;
using InstallyApp.Models;
using InstallyApp.ViewModels;

namespace InstallyApp.Pages
{
    public partial class MainPage : UserControl
    {
        public MainPage()
        {
            InitializeComponent();
            
            DataContext = new MainPageViewModel();
        }

        protected async override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);

            if (DataContext is MainPageViewModel viewModel)
            {
                if (!Design.IsDesignMode)
                {
                    var usersList = await App.DataService.GetAllUsersAsync();
                    
                    foreach (var user in usersList)
                    {
                        MainPageViewModel.Users.Insert(0, user);
                    }
                }
                else
                {
                    MainPageViewModel.Users.Add(new UserEntity { Email = "email@example.com", Password = "password" });
                }
            }
        }
        
        private async void OnAddUserClicked(object? sender, RoutedEventArgs e)
        {
            var navigationParameter = new Dictionary<string, object>
            {
                { nameof(UserEntity), new UserEntity() }
            };

            App.NavigationService.NavigateTo(new ManageUsersPage(navigationParameter));
        }

        private async void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            var selectedUser = e.AddedItems.OfType<UserEntity>().FirstOrDefault();
            if (selectedUser != null)
            {
                var navigationParameter = new Dictionary<string, object>
                {
                    { nameof(UserEntity), selectedUser }
                };

                App.NavigationService.NavigateTo(new ManageUsersPage(navigationParameter));
            }
        }
    }
}