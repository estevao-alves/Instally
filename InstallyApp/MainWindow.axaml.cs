using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using Avalonia.Input;
using DynamicData;
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
using AppSearch = InstallyApp.Pages.AppSearch;

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

            GetPackages.LoadAPIPackages();
            PointerPressed += ClearFocus;
            
            if (!Design.IsDesignMode)
            {
                InitializeServices();
            }
        }
        
        private void ClearFocus(object? sender, PointerPressedEventArgs e) => HiddenButtonToFocus.Focus();
        
        public void InitializeServices()
        {
            Login = new();

            UserEntity user = App.Services.GetService<IUserQuery>().GetAll().FirstOrDefault();

            if (user is not null) App.UserAuthenticated = user;
            else App.Main.Modals.Children.Add(Login);

            var packagesQuery = App.Services.GetService<IPackageQuery>();
            App.Packages = packagesQuery.GetAll().ToList();
            
            AppSearchWindow = new AppSearch();

            LoadCollections();
        }

        public void LoadCollections()
        {
            AddNewCollection = new AddNewCollection();
            
            CollectionList.Children.Clear();
            
            App.Collections = App.Services.GetService<ICollectionQuery>().GetAll().ToList();
            List<CollectionEntity> appCollections = App.Collections;
            
            AddNewCollection.IsVisible = false;
            CollectionList.Children.Add(AddNewCollection);

            if (!appCollections.Any())
            {
                AddNewCollection.AddDefaultCollection();

                AddNewCollection.IsVisible = true;
                Grid.SetColumn(AddNewCollection, 1);
            }
            else
            {
                for (int i = 0; i < appCollections.Count; i++)
                {
                    AppCollection currentCollection = new(i, appCollections[i].Title, appCollections[i].Packages, appCollections[i]);
                    /*
                    SelectedCollection = currentCollection;
                    */
                    CollectionList.Children.Add(currentCollection);

                    Grid.SetColumn(currentCollection, i);
                }

                if(appCollections.Count < 4)
                {
                    AddNewCollection.IsVisible = true;
                    Grid.SetColumn(AddNewCollection, appCollections.Count);
                }
            }
        }
        
        public async void AddAppsToCollection(List<Footer.AppToInstall> list, AppCollection componentCollection, Guid collectionId)
        {
            foreach (Footer.AppToInstall app in list)
            {
                AddToCollectionCommand command = new(app.PackageGuid, collectionId);
                bool resultado = await App.Mediator.Send(command);
                
                componentCollection.AttachAppToCollection(app.Name, app.PackageGuid, collectionId, true);}
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
