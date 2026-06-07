using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
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
    
    public void AddDefaultCollection() => AddCollection("My Collection", App.UserAuthenticated.Guid, new List<PackageEntity>());
    
    private void AddNewCollection_OnPointerPressed(object? sender, PointerPressedEventArgs e) => AddDefaultCollection();
    private void AddNewCollection_OnClick(object? sender, RoutedEventArgs e) => AddDefaultCollection();
    
    public async void AddCollection(string name, Guid user, List<PackageEntity> packages)
    {
        AddCollectionCommand command = new(name, user, packages);
        bool result = await App.Mediator.Send(command);

        if (!result)
            return;

        App.Main.LoadCollections();
    }
}