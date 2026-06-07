using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using InstallyAPI.Commands.CollectionCommands;
using InstallyAPI.Commands.PackageCommands;
using InstallyAPI.Commands.UserCommands;
using InstallyAPI.Models;
using InstallyAPI.Queries.Interfaces;
using InstallyApp.Services;
using InstallyApp.Views.Layout;
using Microsoft.Extensions.DependencyInjection;

namespace InstallyApp.Views.Components;

public partial class AppCollection : UserControl
{
    public CollectionEntity Collection { get; set; }
    public Guid CollectionId { get; set; }
    public int collectionIndex { get; set; }
    private bool _isDeleting;

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

        CollectionId = collection.Guid;
        Collection = collection;
        CollectionTitle.Text = collectionTitle;

        if (collectionPackages != null)
        {
            Collection.Packages = collectionPackages;
        }
    }
    
    public void RenderApps()
    {
        Apps.Children.Clear();

        if (Collection?.Packages == null)
            return;

        foreach (var pkg in Collection.Packages)
        {
            AttachAppToCollection(pkg.Name, pkg.Guid, Collection.Guid);
        }
    }
    
    public async void AttachAppToCollection(string appName, Guid pkgGuid, Guid collectionId)
    {
        var dictionary = App.Packages.ToDictionary(x => x.Guid);
        PackageEntity pkg = dictionary.ContainsKey(pkgGuid) ? dictionary[pkgGuid] : null;
    
        AppCollectionItem newApp = new(appName, pkgGuid, collectionId);
        
        newApp.ParentCollection = this;
        newApp.UpdateVisual(pkgGuid);
    
        newApp.OnRemoveFromCollection += async () =>
        {
            var command = new PkgClearCollectionCommand(pkg);
            bool result = await App.Mediator.Send(command);
            
            if (!result) return;
            
            pkg.CollectionId = null;
            Apps.Children.Remove(newApp);
        };
    
        AddToCollectionCommand command = new(pkg.Guid, Collection.Guid);
        bool result = await App.Mediator.Send(command);

        if (result)
        {
            if (!Collection.Packages.Any(x => x.Guid == pkg.Guid))
            {
                Collection.Packages.Add(pkg);
            }
            
            Apps.Children.Add(newApp);
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
    
    private async void DeleteCollectionButton_OnClick(
        object? sender,
        RoutedEventArgs e)
    {
        if (_isDeleting)
            return;

        _isDeleting = true;

        IsEnabled = false;

        try
        {
            // Copy list before modifying
            var packagesToRemove = Collection.Packages.ToList();

            foreach (var pkg in packagesToRemove)
            {
                if (pkg == null) continue;

                bool result = await App.Mediator.Send(new PkgClearCollectionCommand(pkg));

                if (!result) return;
                
                // UI
                pkg.CollectionId = null;
                ClearPackageOnColletion(pkg);
            }

            // DB
            var command = new DeleteCollectionCommand(CollectionId);
            await App.Mediator.Send(command);
            
            // Update UI
            App.Main.LoadCollections();
        }
        finally
        {
            _isDeleting = false;

            IsEnabled = true;
        }
    }
    
    public async Task ClearPackageOnColletion(PackageEntity package)
    {
        if (package == null) return;

        var command = new PkgClearCollectionCommand(package);
        var result = await App.Mediator.Send(command);

        if (result)
        {
            App.Main.Footer.RemoveAppFromListToInstall(package.Guid);
            SelectionService.Deselect(package.Guid);
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