using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Instally.App.Application;
using InstallyAPI.Commands.CollectionCommands;
using InstallyAPI.Models;
using InstallyAPI.Queries.Interfaces;
using InstallyApp.Views.Components;
using Microsoft.Extensions.DependencyInjection;

namespace InstallyApp.Views.Items;

public partial class AddNewCollection : UserControl
{
    public AddNewCollection()
    {
        InitializeControl();
    }

    public void InitializeControl() => InitializeComponent();
    
    public void AddDefaultCollection() => AddCollection("My Collection", App.UserAuthenticated.Guid, new List<PackageEntity>() );
    
    private void AddNewCollection_OnPointerPressed(object? sender, PointerPressedEventArgs e) => AddDefaultCollection();
    private void AddNewCollection_OnClick(object? sender, RoutedEventArgs e) => AddDefaultCollection();
    
    public async void AddCollection(string name, Guid user, List<PackageEntity> packages)
    {
        InitializeControl();
            
        AddCollectionCommand command = new(name, user, packages);
        bool resultado = await App.Mediator.Send(command);

        if (resultado)
        {
            var collectionQuery = App.Services.GetService<ICollectionQuery>();
            App.Collections = collectionQuery.GetAll().ToList();

            int newIndex = App.Collections.Count - 1;

            AppCollection collection = new(0, name, packages, App.Collections[newIndex]);
            
            App.Main.CollectionList.Children.Add(collection);

            Grid.SetColumn(collection, newIndex);
            
            collection.Apps.Children.Clear();
            
            // Update AddNewCollection Column
            Grid.SetColumn(this, newIndex + 1);

            if (App.Collections.Count >= 4)
            {
                IsVisible = false;
            }
        }
    }
}