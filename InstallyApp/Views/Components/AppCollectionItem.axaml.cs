using Avalonia.Input;
using Avalonia.Media;
using InstallyAPI.Commands.PackageCommands;
using InstallyAPI.Models;
using InstallyApp.DataServices;
using InstallyApp.Services;
using InstallyApp.Views.Layout;

namespace InstallyApp.Views.Components;

public partial class AppCollectionItem : UserControl
{
    string _appName;
    public Guid AppGuid;
    public PackageEntity SelectedPackage;
    public AppCollection ParentCollection { get; set; }

    public delegate void RemoveFromCollection();
    public event RemoveFromCollection OnRemoveFromCollection;

    public string AppName
    {
        get => _appName;
        set
        {
            _appName = value;
            if (Title != null) 
            {
                Title.Text = value;
            }
        }
    }

    public Guid CollectionId { get; set; }
    
    public static event Action<Guid>? SelectionChanged;

    public AppCollectionItem()
    {
            InitializeComponent();
            
            SelectionService.SelectionChanged += OnSelectionChanged;
    }
    
    public AppCollectionItem(string appName, Guid appGuid, Guid collectionId) : this()
    {
        AppName = appName;
        AppGuid = appGuid;
        CollectionId = collectionId;
        AddIcon();
    }

    private void OnSelectionChanged(Guid pkgId)
    {
        if (pkgId == AppGuid)
        {
            UpdateVisual(AppGuid);
        }
    }
    
    public async Task AddIcon()
    {
        Control packagesFavicon = await GetPackages.CatchPackagesFavicon(AppGuid);
        WrapperAppIcon.Child = packagesFavicon;
        
        if (packagesFavicon is Image)
        {
            WrapperAppIcon.Child = packagesFavicon;
        }
        else
        {
            // If it's not an Image, use app first letter
            WrapperAppIcon.Child = new TextBlock()
            {
                Text = AppName.Substring(0, 1),
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                FontSize = 30,
                FontWeight = FontWeight.SemiBold,
                Foreground = (SolidColorBrush)App.Current.Resources["SecondaryText"],
            };
        }
    }

    public void UpdateVisual(Guid package)
    {
        bool isActive = SelectionService.IsSelected(package);

        BorderWrapper.Background = isActive
            ? (SolidColorBrush)App.Current.Resources["PrimaryBackground"]
            : new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
    }
    
    public void ToggleSelection(Guid package, Guid collectionId)
    {
        SelectionService.Toggle(package);

        if (SelectionService.IsSelected(package))
        {
            var selectedPackage = GetPackages.CatchPackage(package);
            
            if (selectedPackage != null)
            {
                SelectedPackage = selectedPackage;
                
                if (App.Main.Footer.AppsListToInstall.Any(x => x.Package.Guid == package)) return;
                App.Main.Footer.AddAppToInstallationList(selectedPackage, collectionId);
            }
        }
        else
        {
            App.Main.Footer.RemoveAppFromListToInstall(package);
        }

        UpdateVisual(package);
    }

    private void AppItem_PointerPressed(object sender, PointerPressedEventArgs e)
    {
        ToggleSelection(AppGuid, CollectionId);
    }

    private async void RemoveButton_Click(object sender, RoutedEventArgs e)
    {
        var pkg = GetPackages.CatchPackage(AppGuid);

        if (pkg == null || ParentCollection == null) return;

        App.Main.AppCollection.ClearPackageOnColletion(pkg);
        
        await ParentCollection?.ClearPackageOnColletion(pkg);
        
        OnRemoveFromCollection?.Invoke();
    }
}