using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Instally.App.Application;
using InstallyAPI.Commands.CollectionCommands;
using InstallyAPI.Commands.PackageCommands;
using InstallyAPI.Commands.UserCommands;
using InstallyAPI.Models;
using InstallyAPI.Queries.Interfaces;
using InstallyApp.Views.Layout;
using Microsoft.Extensions.DependencyInjection;

namespace InstallyApp.Views.Components;

public partial class AppCollection : UserControl
{
    public CollectionEntity Collection { get; set; }
    public List<PackageEntity> CollectionPackages { get; set; }
    public int collectionIndex { get; set; }
    public bool isActive = false;

    public AppCollection()
    {
        InitializeControl();
    }

    public void InitializeControl() => InitializeComponent();
    
    public AppCollection(int collIndex, string collectionTitle, List<PackageEntity> collectionPackages, CollectionEntity collection) : this()
    {
        InitializeControl();
        Apps.Children.Clear();

        collectionIndex = collIndex;

        Collection = collection;
        CollectionTitle.Text = collectionTitle;

        if (collectionPackages != null)
        {
            CollectionPackages = collectionPackages;
            
            foreach (PackageEntity package in collectionPackages)
            {
                AttachAppToCollection(package.Name, package.Guid, package.CollectionId ?? Guid.Empty, true);
            }
        }
    }
    
    public async void AttachAppToCollection(string appName, Guid pkgGuid, Guid collectionId, bool updateCollection)
    {
        var dictionary = App.Packages.ToDictionary(x => x.Guid);
        PackageEntity pkg = dictionary.ContainsKey(pkgGuid) ? dictionary[pkgGuid] : null;

        AppCollectionItem newApp = new(appName, pkgGuid, collectionId);
        
        newApp.ControlAppInInstallationFooter(pkgGuid, collectionId);

        newApp.OnRemoveFromCollection += () =>
        {
            Apps.Children.Remove(newApp);
            Collection.Packages.RemoveAll(p => p.Guid == pkgGuid);
        };

        if (updateCollection)
        {
            AddToCollectionCommand command = new(pkg.Guid, Collection.Guid);
            bool result = await App.Mediator.Send(command);

            if (result)
            {
                Apps.Children.Add(newApp);
            }
        }
    }
    
    private void AddButton_OnClick(object? sender, RoutedEventArgs e)
    {
        App.Main.SelectedCollection = this;
        App.Main.Modals.Children.Add(App.Main.AppSearchWindow);

        App.Main.AppSearchWindow.AppList.Children.Clear();
        App.Main.AppSearchWindow.SearchTextField.Text = string.Empty;
        App.Main.AppSearchWindow.SearchByCategory("All");
        App.Main.AppSearchWindow.ListAppsToCollect = new();
    }
    
    private async void DeleteCollectionButton_OnClick(object? sender, RoutedEventArgs e)
    {
        // Delete Collection from DataBase
        DeleteCollectionCommand command = new(Collection.Guid);
        bool result = await App.Mediator.Send(command);

        if (result)
        {
            var collectionQuery = App.Services.GetRequiredService<ICollectionQuery>();
            App.Collections = collectionQuery.GetAll().ToList();

            int currentColumn = Grid.GetColumn(this);

            App.Main.CollectionList.Children.Remove(this);

            foreach (Control coll in App.Main.CollectionList.Children)
            {
                int elementColumn = Grid.GetColumn(coll);
                if (elementColumn > currentColumn) Grid.SetColumn(coll, elementColumn - 1);
            }
            
            // Remove all the apps in the deleted colletion from installation footer
            App.Main.Footer.RemoveCollectionFromListToInstall(Collection);

            if (App.Collections.Count <= 3) App.Main.AddNewCollection.IsVisible = true;
        }
        
        // Update AddNewCollection Column
        int newIndex = App.Collections.Count - 1;
        
        Grid.SetColumn(App.Main.AddNewCollection, newIndex + 1);

        if (App.Collections.Count >= 4)
        {
            IsVisible = false;
        }
    }
    
    public async void ClearPkgsOnColletion(PackageEntity package)
    {
        var command = new PkgClearCollectionCommand(package);
        var result = await App.Mediator.Send(command);

        if (result)
        {
            App.Main.Footer.RemoveAppFromListToInstall(package.Guid);
        }
    }

    private async void CollectionTitle_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        string newTitle = CollectionTitle.Text;
        
        var command = new UpdateCollectionCommand(Collection.Guid, newTitle);
        var result = await App.Mediator.Send(command);

        if (result)
        {
            Collection.Title = newTitle;
        }
    }

    private void CollectionTitle_OnKeyUp(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            App.Main.HiddenButtonToFocus.Focus();
        }
    }
}