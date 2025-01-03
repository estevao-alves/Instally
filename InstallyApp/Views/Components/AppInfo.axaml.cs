using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using Instally.App.Application;
using InstallyAPI.Models;
using InstallyApp.DataServices;
using InstallyApp.Views.Modals;

namespace InstallyApp.Views.Items;

public partial class AppInfo : UserControl
    {
        bool showMoreActive;
        public AppInfo()
        {
            InitializeComponent();
        }
        
        public async void AtualizarInformacoes(PackageEntity pkg)
        {
            // Remover IsActive (InfoIcon) de todos os outros pacotes
            foreach (AppInSearchList appItem in App.Main.AppSearchWindow.AppList.Children)
            {
                if (pkg.Description.Length < 120)
                {
                    ButtonShowMore.IsVisible = false;
                }
                else
                {
                    ButtonShowMore.IsVisible = true;
                }

                if (appItem.Title.Text != pkg.Name)
                {
                    appItem.InfoIcon.IsChecked = false;
                    appItem.InfoIcon.IsVisible = false;
                }
            }

            // Carregar as informações do pacote em tela
            Control appIcon = await GetPackages.CatchPackagesFavicon(pkg.Guid);
            appIcon.RenderTransform = new TranslateTransform(-2.5F, 0.0F);
            WrapperIcon.Child = appIcon;
            WrapperIcon.Padding = new Thickness(5, 0, 0, 0);

            Description.Text = pkg.Description;
            Publisher.Text = pkg.Publisher;
            LatestVersion.Text = pkg.LatestVersion;

            TagsList.Children.Clear();

            foreach (string tag in pkg.Tags)
            {
                Border border = new();

                TextBlock textBlock = new()
                { 
                    Text = tag,
                };
                
                if (TagsList.Resources["TextBlockItem"] is Style textBlockStyle)
                {
                    textBlock.Styles.Add(textBlockStyle);
                }

                border.Child = textBlock;

                TagsList.Children.Add(border);
            }
        }

        public void Description_ChangeShowText(bool? showMoreActive)
        {
            if (Description.Text.Length > 200)
            {
                ButtonShowMore.IsVisible = true;
            }

            if (showMoreActive is true && ButtonShowMore.Content == "Full Description")
            {
                Description.MaxHeight = double.PositiveInfinity;

                ButtonShowMore.Content = "Show Less";
                ButtonShowMore.Icon = (StreamGeometry)App.Current.Resources["ArrowUpSvg"];
            } else
            {
                Description.MaxHeight = 80;

                ButtonShowMore.Content = "Full Description";
                ButtonShowMore.Icon = (StreamGeometry)App.Current.Resources["ArrowDownSvg"];
            }
        }

        private void ButtonShowMore_OnClick(object? sender, RoutedEventArgs e)
        {
            if(!showMoreActive)
            {
                showMoreActive = false;
                Description_ChangeShowText(showMoreActive);
            }
            {
                showMoreActive = true;
                Description_ChangeShowText(showMoreActive);
            }
        }
    }