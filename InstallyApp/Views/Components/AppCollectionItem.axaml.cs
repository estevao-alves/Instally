using Avalonia.Input;
using Avalonia.Media;
using InstallyAPI.Commands.PackageCommands;
using InstallyAPI.Models;
using InstallyApp.DataServices;
using InstallyApp.Views.Layout;

namespace InstallyApp.Views.Components;

public partial class AppCollectionItem : UserControl
{
    string _appName;
    Guid AppGuid;
    public PackageEntity SelectedWingetPackage;

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

    public bool IsActive { get; set; }
    
    public AppCollectionItem()
    {
            InitializeComponent();
    }
    
    public AppCollectionItem(string appName, Guid appGuid, Guid collectionId) : this()
        {
            AppName = appName;
            AppGuid = appGuid;
            CollectionId = collectionId;
            AdicionarIcone();
        }

        public async void AdicionarIcone()
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

        public void ControlAppInInstallationFooter(Guid package, Guid collectionId)
        {
            IsActive = !IsActive;

            if (IsActive)
            {
                // Add to installation app footer
                PackageEntity selectedPackage = GetPackages.CatchPackages(package);

                if (selectedPackage != null)
                {
                    SelectedWingetPackage = selectedPackage;
                    App.Main.Footer.AddAppToInstallationList(selectedPackage, collectionId);
                }
                
                BorderWrapper.Background = (SolidColorBrush)App.Current.Resources["PrimaryBackground"];
            }
            else
            {
                // Remove from installation app footer
                if (SelectedWingetPackage is not null) App.Main.Footer.RemoveAppFromListToInstall(SelectedWingetPackage.Guid);
                
                BorderWrapper.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
            }
        }

        private void AppItem_PointerPressed(object sender, PointerPressedEventArgs e)
        {
            ControlAppInInstallationFooter(AppGuid, CollectionId);
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            App.Main.AppCollection.ClearPkgsOnColletion(GetPackages.CatchPackages(AppGuid));

            OnRemoveFromCollection();
        }
}