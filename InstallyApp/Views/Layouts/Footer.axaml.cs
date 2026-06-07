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
    public List<AppToInstall> AppsListToInstall { get; set; } = new();
    
    public Footer()
    {
        InitializeComponent();
        
        this.Loaded += OnLoaded;
    }
    
    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        UpdateInstallationList();
    }
    
    public class AppToInstall
    {
        public PackageEntity Package;
        public Guid CollectionGuid;

        public AppToInstall(PackageEntity package, Guid collectionGuid)
        {
            Package = package;
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
    
    public void UpdateInstallationList()
    {
        // Update apps visually on footer
        InstallationList.Children.Clear();

        foreach (AppToInstall app in AppsListToInstall)
        {
            Button Icone = BuildAppIcon(app.Package.Name, app.Package.Guid);
            InstallationList.Children.Add(Icone);
        }
    }
    
    public Button AddAppToInstallationList(PackageEntity pkg, Guid collectionId)
    {
        InitializeComponent();

        // Add on variable AppsListToInstall
        AppsListToInstall.Add(new AppToInstall(pkg, collectionId));
        
        Debug.WriteLine(AppsListToInstall);

        // Add visual Icons on Footer
        Button Icone = BuildAppIcon(pkg.Name, pkg.Guid);
        InstallationList.Children.Add(Icone);

        UpdateInstallationList();

        return Icone;
    }
    
    public void RemoveAppsFromListToInstall(IEnumerable<Guid> packageGuids)
    {
        var guidSet = packageGuids.ToHashSet();

        AppsListToInstall.RemoveAll(x => guidSet.Contains(x.Package.Guid));

        UpdateInstallationList();
    }
    
    public void RemoveAppFromListToInstall(Guid packageGuid)
    {
        // Remove from variable AppsListToInstall
        AppsListToInstall = new(AppsListToInstall.FindAll(item => item.Package.Guid != packageGuid));
        
        UpdateInstallationList();
    }

    public void RemoveCollectionFromListToInstall(CollectionEntity collection)
    {
        foreach (PackageEntity collectionPkg in collection.Packages)
        {
            RemoveAppFromListToInstall(collectionPkg.Guid);
        }
        
        UpdateInstallationList();
    }
    
    private void AppInstall_OnClick(object? sender, RoutedEventArgs e)
    {
        var installation = new AppInstallation();

        installation.AppsListToInstall = new List<AppToInstall>(AppsListToInstall);

        _ = installation.StartChecking();
    }
}