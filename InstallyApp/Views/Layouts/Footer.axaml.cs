using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using InstallyAPI.Commands.PackageCommands;
using InstallyAPI.Models;
using InstallyApp.DataServices;
using InstallyApp.Pages;
using InstallyApp.Views.Controls;

namespace InstallyApp.Views.Layout;

public partial class Footer : UserControl
{
    public AppInstallation appInstalationWindow;
    
    public Footer()
    {
        InitializeComponent();
        
        appInstalationWindow = new AppInstallation();
    }
    
    public class AppToInstall
    {
        public string Name;
        public string WingetId;
        public Guid PackageGuid;
        public Guid CollectionGuid;

        public AppToInstall(string name, string wingetId, Guid packageGuid, Guid collectionGuid)
        {
            Name = name;
            WingetId = wingetId;
            PackageGuid = packageGuid;
            CollectionGuid = collectionGuid;
        }
    }
    
    private Button BuildAppIcon(string pkgName, Guid pkgGuid)
    {
        // Build apps visually on footer
        Button borderWrapper = new()
        {
            Background = (SolidColorBrush)App.Current.Resources["PrimaryBackground"],
            Width = 50,
            Height = 50,
            Margin = new Thickness(12, 0, 0, 0)
        };

        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var packagesFavicon = await GetPackages.CatchPackagesFavicon(pkgGuid);

            // Replace the content with the image or fallback
            if (packagesFavicon != null)
            {
                borderWrapper.Content = packagesFavicon;
            }
            else
            {
                borderWrapper.Content = new TextBlock()
                {
                    Text = pkgName.Substring(0, 1),
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                    FontSize = 30,
                    FontWeight = FontWeight.SemiBold,
                    Foreground = (SolidColorBrush)App.Current.Resources["SecondaryText"]
                };
            }
        });

        return borderWrapper;
    }
    
    private void AtualizarLista()
    {
        // Update apps visually on footer
        InstallationList.Children.Clear();

        foreach (AppToInstall app in appInstalationWindow.AppsListToInstall)
        {
            Button Icone = BuildAppIcon(app.Name, app.PackageGuid);
            InstallationList.Children.Add(Icone);
        }
    }
    
    public Button AddAppToInstallationList(PackageEntity pkg, Guid collectionId)
    {
        InitializeComponent();

        // Add on variable AppsListToInstall
        appInstalationWindow.AppsListToInstall.Add(new AppToInstall(pkg.Name, pkg.WingetId, pkg.Guid, collectionId));
        
        Debug.WriteLine(appInstalationWindow.AppsListToInstall);

        // Add visual Icons on Footer
        Button Icone = BuildAppIcon(pkg.Name, pkg.Guid);
        InstallationList.Children.Add(Icone);

        AtualizarLista();

        return Icone;
    }
    
    public void RemoveAppFromListToInstall(Guid pkgGuid)
    {
        // Remove from variable AppsListToInstall
        appInstalationWindow.AppsListToInstall = new(appInstalationWindow.AppsListToInstall.FindAll(item => item.PackageGuid != pkgGuid));
        
        AtualizarLista();
    }

    public void RemoveCollectionFromListToInstall(CollectionEntity collection)
    {
        foreach (PackageEntity collectionPkg in collection.Packages)
        {
            RemoveAppFromListToInstall(collectionPkg.Guid);
        }
        
        AtualizarLista();
    }
    
    private void AppInstall_OnClick(object? sender, RoutedEventArgs e)
    {
        appInstalationWindow.StartVerification();
    }
}