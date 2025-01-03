using System.Collections.ObjectModel;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using InstallyAPI.Commands.PackageCommands;
using InstallyAPI.Models;
using InstallyApp.DataServices;
using InstallyApp.Views.Components;
using InstallyApp.Views.Layout;
using InstallyApp.Views.Modals;
using Button = Avalonia.Controls.Button;
using FontWeight = Avalonia.Media.FontWeight;
using Image = Avalonia.Controls.Image;
using SelectionChangedEventArgs = Avalonia.Controls.SelectionChangedEventArgs;
using SolidColorBrush = Avalonia.Media.SolidColorBrush;
using Thickness = Avalonia.Thickness;

namespace InstallyApp.Pages;

public partial class AppSearch : UserControl
{
    string? DefaultTextSearch;
    string? ChosenCategory;

    List<PackageEntity> PackagesFound { get; set; } = new();
    int ResultLimit = 0;
    
    public ObservableCollection<string> Categories { get; private set; } = new();
    
    public Dictionary<string, List<string>> CategoriesDict { get; set; }
    
    public string SelectedCategoryName  { get; set; }

    public event EventHandler<ScrollChangedEventArgs> ViewChanged;

    public List<Footer.AppToInstall> ListAppsToCollect;

    public AppSearch()
    {
        InitializeControl();
        DataContext = this;
        
        CategoriesDict = new Dictionary<string, List<string>>()
        {
            { "All", new List<string> { "" } },
            { "Games", new List<string> { "gaming", "game", "launcher" } },
            { "Music", new List<string> { "audio", "song", "playlist", "radio", "podcast", "recording" } },
            { "Art", new List<string> { "design", "drawing", "svg", "vector-graphics", "paint", "photo", "3d" } },
            { "Web", new List<string> { "chat", "mail", "wifi", "cloud", "network", "security", "connection", "browser" } },
            { "Utility", new List<string> { "dictionary", "education", "productivity", "display", "tool", "publisher", "customization", "disk", "notes", "word", "compression", "document", "keyboardmanager", "boot", "conversion" } },
            { "Develop", new List<string> { "ide", "development", "developer-tools", "code", "sqlite", "python", "runtime", "terminal", "javascript", "jetbrains" } }
        };
        
        SelectedCategoryName = "All";
        
        foreach (var category in CategoriesDict.Keys)
        {
            Categories.Add(category);
        }
    }

    public void InitializeControl()
    {
        InitializeComponent();
    }
    private void AppSearch_OnInitialized(object? sender, EventArgs e)
    {
        InitializeControl();
        
        AppList.Children.Clear();
        SearchAppsSelected.Children.Clear();

        DefaultTextSearch = SearchTextField.Text;
        
        ScrollBar.ScrollChanged += OnScrollCheckScrollPosition;
    }
    
    private void OnScrollCheckScrollPosition(object sender, EventArgs e)
    {
        if (ScrollBar != null)
        {
            double verticalOffset = ScrollBar.Offset.Y;
            double extentHeight = ScrollBar.Extent.Height;
            double viewportHeight = ScrollBar.Viewport.Height;

            // Check if the user has scrolled near the bottom of the content
            if (verticalOffset + viewportHeight > extentHeight - 200)
            {
                SearchPackages();
            }
        }
    }
    
    private void CloseButton_OnClick(object? sender, RoutedEventArgs e)
    {
        App.Main.Modals.Children.Clear();
        SearchAppsSelected.Children.Clear();
    }
    
    public void SearchPackages()
    {
        if (Design.IsDesignMode) return;

        string textTyped = SearchTextField.Text ?? string.Empty;
        string? filtro = textTyped.Length > 0 ? (textTyped != DefaultTextSearch ? textTyped : null) : null;
        
        List<PackageEntity> packageModels = GetPackages.CatchPackages(filtro, ChosenCategory, ResultLimit, 42);

        PackagesFound = packageModels;
        
        ResultLimit += 42;

        foreach (PackageEntity package in PackagesFound)
        {
            AppInSearchList app = new(package.Name, package.Guid, AppInSearchList.VerifyAppAdded(package.Guid) is not null);
            AppList.Children.Add(app);
        }
    }

    public void SearchByCategory(string chosenCategory)
    {
        AppList.Children.Clear();

        ResultLimit = 0;

        if (chosenCategory == "All") ChosenCategory = null;
        else ChosenCategory = chosenCategory;

        SearchPackages();
        ScrollBar.ScrollToHome();
    }

    private void Search_OnClick(object? sender, RoutedEventArgs e)
    {
        ResultLimit = 0;
        AppList.Children.Clear();

        SearchPackages();
        ScrollBar.ScrollToHome();
    }

    public Button AddApp(PackageEntity pkg)
    {
        ListAppsToCollect.Add(
            new Footer.AppToInstall(pkg.Name, pkg.WingetId,pkg.Guid, App.Main.SelectedCollection.Collection.Guid));
        
        Button borderWrapper = new();

        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            Control? packagesFavicon = await GetPackages.CatchPackagesFavicon(pkg.Guid);

            if (packagesFavicon is Image)
            {
                borderWrapper.Content = packagesFavicon;
            }
            else
            {
                // If it's not an Image, return app first letter
                borderWrapper.Content = new TextBlock()
                {
                    Text = pkg.Name.Substring(0, 1),
                };
            }

            SearchAppsSelected.Children.Add(borderWrapper);
        });
        
        return borderWrapper;
    }

    public void RemoverApp(Button appItemWithBorder, string wingetId)
    {
        ListAppsToCollect = ListAppsToCollect.FindAll(item => item.WingetId != wingetId);
        SearchAppsSelected.Children.Remove(appItemWithBorder);
    }
    
    private void SearchTextField_KeyUp(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            ResultLimit = 0;
            AppList.Children.Clear();

            SearchPackages();
            ScrollBar.ScrollToHome();
            ComboBoxCategory.Focus();
        }
    }
    
    public void AppList_ChangeColumns()
    {
        if (AppList.Bounds.Width < 1100) 
            AppList.Columns = 4;
        else if (AppList.Bounds.Width < 1600) 
            AppList.Columns = 6;
        else 
            AppList.Columns = 8;

        if (AppInfo.IsVisible) 
            AppList.Columns = AppList.Columns - 2;
    }
    
    private void AppList_OnSizeChanged(object? sender, SizeChangedEventArgs e) => AppList_ChangeColumns();

    private async void AddApps_OnClick(object? sender, RoutedEventArgs e)
    {
        foreach (Footer.AppToInstall appsToCollect in ListAppsToCollect)
        {
            PackageEntity package = App.Packages.Where(p => p.WingetId == appsToCollect.WingetId).FirstOrDefault();
            
            App.Main.SelectedCollection.CollectionPackages.Add(package);
            
            AddToCollectionCommand command = new(appsToCollect.PackageGuid, App.Collections[0].Guid);
            bool result = await App.Mediator.Send(command);
        }
        
        App.Main.AddAppsToCollection(ListAppsToCollect, App.Main.SelectedCollection, App.Collections[0].Guid);

        App.Main.AppSearchWindow.SearchAppsSelected.Children.Clear();

        App.Main.Modals.Children.Clear();
    }

    private void ComboBoxCategory_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (ComboBoxCategory.SelectedItem is string selectedCategoryName)
        {
            SearchByCategory(selectedCategoryName);
        }
        else
        {
            SearchByCategory(null);
        }
    }
}