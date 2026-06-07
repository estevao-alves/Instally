using System.IO;
using InstallyApp.Utils.Functions;
using InstallyApp.Views.Layout;
using Avalonia.Media;
using InstallyAPI.Models;
using InstallyApp.DataServices;
using InstallyApp.Services;

namespace InstallyApp.Pages
{
    public partial class AppInstallation : UserControl
    {
        public List<Footer.AppToInstall> AppsListToInstall { get; set; } = new();
        
        private enum InstallationState
        {
            Checking,
            Installing,
            Paused,
            Error
        }
        
        private enum DialogButtonType
        {
            Continue,
            Retry,
            Completed,
            Notice
        }
        
        public AppInstallation()
        {
            DataContext = this;
            InitializeComponent();
        }
        
        // --- Main Workflow Methods
        private void SetInstallationState(
            InstallationState state,
            string title = "",
            string details = "",
            bool inProgress = true,
            DialogButtonType dialogType = DialogButtonType.Continue)
        {
            ApplyStateUI(state, title, details);
            if (!inProgress) ApplyDialogButtonTypeUI(dialogType);
        }
        
        public async Task StartChecking()
        {
            App.Main.Modals.Children.Add(this);
            
            SetInstallationState(InstallationState.Checking);

            if (AppsListToInstall.Count == 0)
            {
                SetInstallationState(InstallationState.Paused);
                await WaitForUserConfirmation("Select at least one app to install", "", DialogButtonType.Notice);

                return;
            }

            List<string> alreadyInstalledApps = new();
            
            var packageSource = PlatformService.PackageSource;

            await AssureFlatpakIsReady();
            
            SetInstallationState(InstallationState.Checking, "Flathub");

            for (int i = 0; i < AppsListToInstall.Count; i++)
            {
                var app = AppsListToInstall[i];
                SetInstallationState(InstallationState.Checking, $"{app.Package.Name}", $"Checking ({i + 1}/{AppsListToInstall.Count})");

                string packageId = GetPackageId(app);
                CommandResult result;
                
                if (packageSource == "Winget")
                {
                    result = await Command.Execute("winget", $"list -q {packageId} --accept-source-agreements");
                }
                else if (packageSource == "Flatpak")
                {
                    result = await Command.Execute("flatpak", "list");
                }
                else
                {
                    throw new Exception($"Unsupported package manager: {packageSource}");
                }

                if (!string.IsNullOrEmpty(packageId) && result.Output.Contains(packageId))
                {
                    alreadyInstalledApps.Add(packageId);
                }
            }

            if (AppsListToInstall.Count == alreadyInstalledApps.Count)
            {
                SetInstallationState(InstallationState.Paused);
                await WaitForUserConfirmation("All selected apps are already installed", "", DialogButtonType.Notice);

                return;
            }

            if (alreadyInstalledApps.Count > 0)
            {
                SetInstallationState(InstallationState.Paused);
                
                string title = $"{alreadyInstalledApps.Count} app{(alreadyInstalledApps.Count > 1 ? "s" : "")} already installed, {AppsListToInstall.Count - alreadyInstalledApps.Count} to go.";
                string details = "Continue installation?";
                if (!await WaitForUserConfirmation(title, details, DialogButtonType.Continue))
                    return;
            }

            var appsToInstall = AppsListToInstall.FindAll(app =>
            {
                var id = GetPackageId(app);
                return !alreadyInstalledApps.Contains(id);
            });
            
            await StartInstallation(appsToInstall);
        }

        private async Task StartInstallation(List<Footer.AppToInstall> apps)
        {
            List<PackageEntity> failedApps = new List<PackageEntity>();
            
            for (int i = 0; i < apps.Count; i++)
            {
                var app = apps[i];
                
                try
                {
                    SetInstallationState(InstallationState.Installing, $"{app.Package.Name}", $"Installing ({i + 1}/{apps.Count})", inProgress: true);

                    ProgressBar.Value = ((i + 1) * 100) / apps.Count;
                    string packageId = GetPackageId(app);
                    CommandResult result;

                    var packageManager = PlatformService.PackageSource;
                    
                    if (packageManager == "Winget")
                    {
                        result = await Command.Execute("winget",
                            $"install {packageId} --accept-source-agreements --accept-package-agreements");
                    }
                    else if (packageManager == "Flatpak")
                    {
                        result = await Command.Execute("flatpak", $"install -y flathub {packageId}");
                    }
                    else
                    {
                        throw new Exception($"Unsupported package manager: {packageManager}");
                    }
                    
                    if (result.ExitCode != 0 || string.IsNullOrEmpty(packageId) || !result.Output.Contains(packageId))
                    {
                        throw new Exception($"{result.Command}\n\t{result.Output}\n\t{result.Error}");
                    }
                }
                catch (Exception ex)
                {
                    SetInstallationState(InstallationState.Error, ex.Message);
                    
                    failedApps.Add(apps[i].Package);
                    
                    if (apps.Count == 1)
                    {
                        await WaitForUserConfirmation($"{app.Package.Name} was not installed", $"{ex.Message}", DialogButtonType.Notice);
                    }
                    else
                    {
                        bool goToNextApp = await WaitForUserConfirmation($"{app.Package.Name} was not installed", $"{ex.Message}", DialogButtonType.Retry);
                        
                        if (goToNextApp) continue;
                        
                        failedApps.Remove(apps[i].Package);
                        i--;
                    }
                }
            }
            
            SetInstallationState(InstallationState.Paused);

            var successfulApps = apps.FindAll(app => !failedApps.Contains(app.Package));
            
            if (failedApps.Count == apps.Count)
            {
                await WaitForUserConfirmation(
                    "Installation failed",
                    failedApps.Count > 1
                        ? $"None of the {apps.Count} selected apps could be installed"
                        : $"{failedApps.First().Name} could not be installed",
                    DialogButtonType.Notice);

                return;
            }

            string title = successfulApps.Count switch
            {
                1 => $"{successfulApps[0].Package.Name} was successfully installed!",
                _ when failedApps.Count == 0 => "All apps were successfully installed!",
                _ => $"{successfulApps.Count} apps were successfully installed!"
            };

            string details = failedApps.Count switch
            {
                0 => "",
                1 => $"{failedApps[0].Name} failed",
                _ => $"{failedApps[0].Name} + {failedApps.Count - 1} more failed"
            };

            await WaitForUserConfirmation(
                title,
                details,
                DialogButtonType.Completed);
            
            foreach (var app in successfulApps)
            {
                SelectionService.Deselect(app.Package.Guid);
            }
            
            App.Main.Footer.RemoveAppsFromListToInstall(successfulApps.Select(x => x.Package.Guid));
        }
        //
        
        // --- Modal UI Management
        private Task<bool> WaitForUserConfirmation(
            string title,
            string details,
            DialogButtonType type
        )
        {
            var tcs = new TaskCompletionSource<bool>();

            ApplyDialogContentUI(title, details);
            ApplyDialogButtonTypeUI(type);

            void ConfirmHandler(object? sender, RoutedEventArgs e)
            {
                Confirm.Click -= ConfirmHandler;
                Cancel.Click -= CancelHandler;
                
                if (type == DialogButtonType.Completed || type == DialogButtonType.Notice) App.Main.Modals.Children.Clear();
                
                tcs.TrySetResult(true);
            }

            void CancelHandler(object? sender, RoutedEventArgs e)
            {
                Confirm.Click -= ConfirmHandler;
                Cancel.Click -= CancelHandler;
                
                if (type != DialogButtonType.Retry) App.Main.Modals.Children.Clear();
                
                tcs.TrySetResult(false);
            }

            Confirm.Click += ConfirmHandler;
            Cancel.Click += CancelHandler;

            return tcs.Task;
        }
        //
        
        // --- UI State Methods
        private void ApplyStateUI(
            InstallationState state,
            string title,
            string details)
        {

            ApplyDialogContentUI(title, details);
            ResetStateUI();
            
            switch (state)
            {
                case InstallationState.Checking:
                case InstallationState.Installing:
                    ProgressBar.IsVisible = true;
                    Buttons.IsVisible = false;
                    break;
                
                case InstallationState.Paused:
                    break;

                case InstallationState.Error:
                    TextDetails.IsEnabled = true;
                    TextDetails.FontSize = 14;
                    TextDetails.TextAlignment = TextAlignment.Left;
                    TextDetails.Foreground = (SolidColorBrush)App.Current.Resources["SecondaryText"];
                    break;

                default:
                    throw new InvalidOperationException($"State '{state}' is not handled.");
            }
        }
        
        private void ApplyDialogButtonTypeUI(DialogButtonType type)
        {
            Buttons.IsVisible = true;
            Confirm.IsVisible = true;
            Cancel.IsVisible = true;

            Confirm.Classes.Clear();
            Cancel.Classes.Clear();

            // Default button style
            Confirm.Classes.Add("action");
            Confirm.Classes.Add("text-only");

            Cancel.Classes.Add("base");
            Cancel.Classes.Add("text-only");

            switch (type)
            {
                case DialogButtonType.Continue:
                    Cancel.Content = "Cancel";
                    Confirm.Content = "Continue";
                    break;

                case DialogButtonType.Retry:
                    Cancel.Content = "Retry";
                    Confirm.Content = "Go to next app";
                    break;

                case DialogButtonType.Completed:
                    Confirm.Content = "Confirm";
                    Cancel.IsVisible = false;

                    Confirm.Classes.Clear();
                    Confirm.Classes.Add("positive");
                    break;

                case DialogButtonType.Notice:
                    Confirm.Content = "Close";
                    Cancel.IsVisible = false;
                    break;
            }
        }
        
        private void ApplyDialogContentUI(string title, string details)
        {
            Title.Text = title;
            Title.IsVisible = !string.IsNullOrWhiteSpace(title);

            DetailsScrollViewer.IsVisible = !string.IsNullOrWhiteSpace(details);
            TextDetails.IsVisible = !string.IsNullOrWhiteSpace(details);
            TextDetails.Text = details;
        }
        
        private void ResetStateUI()
        {
            ProgressBar.IsVisible = false;
            Buttons.IsVisible = false;

            TextDetails.FontSize = 15;
            TextDetails.IsEnabled = false;
            TextDetails.TextAlignment = TextAlignment.Center;
        }
        //
        
        // --- Helper Methods
        private string? GetPackageId(Footer.AppToInstall app)
        {
            var source = PlatformService.PackageSource;

            return app.Package.PackageIds.TryGetValue(source, out var id)
                ? id
                : null;
        }

        private async Task AssureFlatpakIsReady()
        {
            if (!OperatingSystem.IsLinux()) return;

            try
            {
                // Flatpak verification
                
                var result = await Command.Execute("flatpak", "--version");
                
                if (result.ExitCode != 0)
                {
                    SetInstallationState(InstallationState.Error);
                    
                    bool confirmed = await WaitForUserConfirmation(
                        "Flatpak wasn't found\n" +
                        "Press continue to install it using:",
                        $"\t\t$  sudo {GetFlatpakInstallCommand()}",
                        DialogButtonType.Continue
                    );

                    if (!confirmed) return;

                    SetInstallationState(InstallationState.Installing, "Flatpak");
                    await Command.Execute($"pkexec", GetFlatpakInstallCommand());
                }
                
                // Flathub repository verification
                
                var remotes = await Command.Execute("flatpak", "remotes");
                
                if (!remotes.Output.Contains("flathub"))
                {
                    SetInstallationState(InstallationState.Error);

                    bool confirmed = await WaitForUserConfirmation(
                        "Flathub is not configured\n" +
                        "Press continue to add it using:",
                        $"\t\t$  remote-add --if-not-exists flathub https://flathub.org/repo/flathub.flatpakrepo",
                        DialogButtonType.Continue
                    );
                    
                    if (!confirmed) return;
                    
                    SetInstallationState(InstallationState.Installing, "Flathub");
                    await Command.Execute("flatpak", "remote-add --if-not-exists flathub https://flathub.org/repo/flathub.flatpakrepo");
                }
            }
            catch (Exception ex)
            {
                SetInstallationState(InstallationState.Error, ex.Message);
            }
        }
        
        private static string GetFlatpakInstallCommand()
        {
            if (!OperatingSystem.IsLinux()) return null;

            var distro = File.ReadLines("/etc/os-release")
                .FirstOrDefault(x => x.StartsWith("ID="))
                ?.Replace("ID=", "")
                .Replace("\"", "")
                .Trim();

            return distro switch
            {
                "fedora" => "dnf install -y flatpak",
                "ubuntu" => "apt install -y flatpak",
                "debian" => "apt install -y flatpak",
                "arch" => "pacman -S --noconfirm flatpak",
                "opensuse-tumbleweed" => "zypper install -y flatpak",
                "opensuse-leap" => "zypper install -y flatpak",
                _ => throw new Exception($"Unsupported Linux distribution: {distro}")
            };
        }
        //

        // --- Event Handlers
        private void CloseButton_OnClick(object? sender, RoutedEventArgs e)
        {
            if (PlatformService.PackageSource == "Winget")
            {
                Command.Execute("powershell", "Stop-Process -Name winget -Force");
            }
            
            App.Main.Modals.Children.Clear();
        }
    }
}
