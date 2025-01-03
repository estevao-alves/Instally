using Avalonia.Input;
using Avalonia.Media;
using InstallyAPI.Models;
using InstallyApp.DataServices;
using PointerEventArgs = Avalonia.Input.PointerEventArgs;

namespace InstallyApp.Views.Modals;

public partial class AppInSearchList : UserControl
    {
        public bool IsActive;

        public string PkgName;
        public Guid PkgGuid = Guid.Parse("090FCAE2-A553-49AF-8AB0-916D4EC64655");
        Button appInListToInstall;

        public AppInSearchList()
        {
            InitializeComponent();

            InfoIcon.IsVisible = false;
        }
        
        public AppInSearchList(string pkgName, Guid pkgGuid, bool packageAlreadyAdded) : this()
        {
            InfoIcon.IsVisible = false;

            PkgName = pkgName;
            PkgGuid = pkgGuid;
            LoadAppInfo(pkgName, pkgGuid);
            IconAlreadyAdded(packageAlreadyAdded);
        }
        
        public void IconAlreadyAdded(bool state)
        {
            IsAddedBorder.IsVisible = state;
        }

        public async void LoadAppInfo(string pkgName, Guid pkgGuid)
        {
            Control? appIcon = await GetPackages.CatchPackagesFavicon(pkgGuid);
            if(appIcon is not null)
            {
                appIcon.RenderTransform = new TranslateTransform(-2.5F, 0.0F);
                WrapperIcon.Child = appIcon;
                WrapperIcon.Padding = new Thickness(5, 0, 0, 0);
            }

            Title.Text = pkgName;
        }

        private async void WrapperAppItem_PointerPressed(object sender, PointerPressedEventArgs e)
        {
            string packageAddedName = VerifyAppAdded(PkgGuid);
                
            if (VerifyAppAdded(PkgGuid) is not null)
            {
                AlertPopupText.Text = $"App already added in \"{packageAddedName}\"";

                AlertPopup.IsOpen = true;
                await Task.Delay(2000);
                AlertPopup.IsOpen = false;

                return;
            }
            
            PackageEntity pkg = GetPackages.CatchPackages(PkgGuid);
            
            if (IsActive) {
                IsActive = false;
                WrapperAppItem.Background = new SolidColorBrush(Color.FromArgb(0,0,0,0));

                App.Main.AppSearchWindow.RemoverApp(appInListToInstall, pkg.WingetId);
            }
            else
            {
                IsActive = true;
                WrapperAppItem.Background = (SolidColorBrush)App.Current.Resources["PrimaryBackground"];

                // Add to instalation list
                appInListToInstall = App.Main.AppSearchWindow.AddApp(pkg);
            }
        }

        private void WrapperAppItem_OnPointerEntered(object? sender, PointerEventArgs e)
        {
            InfoIcon.IsVisible = true;
        }

        private void WrapperAppItem_OnPointerExited(object? sender, PointerEventArgs e)
        {
            if (!InfoIcon.IsChecked ?? false) InfoIcon.IsVisible = false;
        }

        private void InfoIcon_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
        {
            if (InfoIcon.IsChecked ?? false)
            {
                PackageEntity? pkg = GetPackages.CatchPackages(PkgGuid);
                
                App.Main.AppSearchWindow.AppInfo.AtualizarInformacoes(pkg);

                App.Main.AppSearchWindow.AppInfo.IsVisible = true;
            }
            else App.Main.AppSearchWindow.AppInfo.IsVisible = false;
            
            App.Main.AppSearchWindow.AppList_ChangeColumns();

            e.Handled = true;
        }
        
        public static string? VerifyAppAdded(Guid pkgGuid)
        {
            foreach (var collection in App.Collections)
            {
                PackageEntity? pkg = collection.Packages.Find(pkg => pkg.Guid == pkgGuid);
                if (pkg is not null) return collection.Title;
            }
            
            return null;
        }
    }