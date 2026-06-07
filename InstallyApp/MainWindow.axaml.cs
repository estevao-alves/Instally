using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using Avalonia.Input;
using DynamicData;
using InstallyAPI.Commands.CollectionCommands;
using InstallyAPI.Commands.PackageCommands;
using InstallyAPI.Models;
using InstallyAPI.Queries.Interfaces;
using InstallyApp.DataServices;
using InstallyApp.Models;
using InstallyApp.Pages;
using InstallyApp.Views.Components;
using InstallyApp.Views.Items;
using InstallyApp.Views.Layout;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Avalonia.Diagnostics;
using Avalonia.Platform;

namespace InstallyApp
{
    public partial class MainWindow : Window
    {
        public LoginPage Login;
        public AppSearch AppSearchWindow;
        public AppCollection SelectedCollection;
        public AddNewCollection AddNewCollection;

        public ObservableCollection<UserEntity> Users { get; set; }

        public MainWindow()
        {
            App.Main = this;
            InitializeComponent();
            
            Icon = new WindowIcon(AssetLoader.Open(new Uri("avares://InstallyApp/Assets/instally-logo.png")));
            
            PointerPressed += ClearFocus;

            CollectionList.Children.Clear();
            
            if (!Design.IsDesignMode)
            {
                InitializeServices();
            }
            
            // Command output window = Ctrl + Shift + T
            // Css-like UI Window = F12
            #if DEBUG
            this.AttachDevTools();
            #endif
        }
        
        private void ClearFocus(object? sender, PointerPressedEventArgs e) => HiddenButtonToFocus.Focus();
        
        public async Task InitializeServices()
        {
            ProgressBar.IsVisible = true;
            await Task.Yield();
            
            // Services
            try
            {
                var apiHost = App.Services.GetRequiredService<ApiHostService>();
                await apiHost.StartAsync();

                await Task.Yield();
                
                Login = new();
                await LoadUsers();

                await LoadPackages();

                AppSearchWindow = new AppSearch();

                LoadCollections();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
            //
            
            ProgressBar.IsVisible = false;
        }
        
        public async Task LoadPackages()
        {
            await Task.Run(async () =>
            {
                var collectionQuery = App.Services.GetRequiredService<ICollectionQuery>();
                App.Collections = collectionQuery.GetAll().ToList();

                var packagesQuery = App.Services.GetRequiredService<IPackageQuery>();
                App.Packages = packagesQuery.GetAll().ToList();

                if (App.Packages.Count < 3000)
                {
                    using var scope = App.Services.CreateScope();
                    var sync = scope.ServiceProvider.GetRequiredService<SyncService>();

                    await sync.SyncPackages();
                }
            });
        }
        
        private async Task LoadUsers()
        {
            var api = new ApiService();
            var users = await api.GetUsers();

            Users = new ObservableCollection<UserEntity>(users);
            
            // Login First User
            if (Users is not null) App.UserAuthenticated = Users.FirstOrDefault();
            else App.Main.Modals.Children.Add(Login);
            //
        }

        public void LoadCollections()
        {
            CollectionList.Children.Clear();

            var collections = App.Services
                .GetRequiredService<ICollectionQuery>()
                .GetAll()
                .OrderBy(c => c.CreatedAt)
                .ToList();

            var packages = App.Services
                .GetRequiredService<IPackageQuery>()
                .GetAll()
                .ToList();

            // Rebuild package lists from Package.CollectionId
            foreach (var collection in collections)
            {
                collection.Packages = packages
                    .Where(p => p.CollectionId == collection.Guid)
                    .ToList();
            }

            App.Collections = collections;

            if (collections.Count == 0)
            {
                AddNewCollection = new AddNewCollection();

                CollectionList.Children.Add(AddNewCollection);

                AddNewCollection.AddDefaultCollection();

                return;
            }

            for (int i = 0; i < collections.Count; i++)
            {
                var collection = new AppCollection(
                    i,
                    collections[i].Title,
                    collections[i].Packages,
                    collections[i]);

                collection.RenderApps();

                CollectionList.Children.Add(collection);

                Grid.SetColumn(collection, i);
            }

            AddNewCollection = new AddNewCollection();

            AddNewCollection.IsVisible = collections.Count < 4;

            CollectionList.Children.Add(AddNewCollection);

            Grid.SetColumn(AddNewCollection, collections.Count);
        }

        private void InputElement_OnKeyDown(object? sender, KeyEventArgs e)
        {
            if ((e.KeyModifiers.HasFlag(KeyModifiers.Control)) && (e.KeyModifiers.HasFlag(KeyModifiers.Shift)))
            {
                if (e.Key == Key.T)
                {
                    App.Debug = new();
                    App.Debug.Show();
                }
            }
        }
    }
}
