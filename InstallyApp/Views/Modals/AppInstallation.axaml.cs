using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using InstallyApp.Utils.Functions;
using InstallyApp.Views.Layout;

namespace InstallyApp.Pages
{
    public partial class AppInstallation : UserControl
    {
        public List<Footer.AppToInstall> AppsListToInstall { get; set; } = new();
        public EnumState CurrentState { get; private set; }

        public enum EnumState
        {
            Checking,
            Waiting,
            Installing,
            Error
        }

        private readonly Dictionary<EnumState, Action<string, bool>> _stateHandlers;

        public AppInstallation()
        {
            DataContext = this;
            InitializeComponent();

            _stateHandlers = new Dictionary<EnumState, Action<string, bool>>
            {
                { EnumState.Checking, HandleCheckingState },
                { EnumState.Waiting, HandleWaitingState },
                { EnumState.Installing, HandleInstallingState },
                { EnumState.Error, HandleErrorState }
            };
        }

        public void SetInstallationState(EnumState state, string details = "", bool inProgress = true)
        {
            CurrentState = state;

            if (_stateHandlers.TryGetValue(state, out var handler))
            {
                handler.Invoke(details, inProgress);
            }
            else
            {
                throw new InvalidOperationException($"State '{state}' is not handled.");
            }
        }

        private void HandleCheckingState(string details, bool inProgress)
        {
            UpdateUIState("Checking...", details, 14, showProgressBar: true, showButtons: false);
        }

        private void HandleWaitingState(string details, bool inProgress)
        {
            UpdateUIState("", details, 18, showProgressBar: false, showButtons: true);
            Confirm.IsVisible = true;
            /*Cancel.IsVisible = true;*/
        }

        private void HandleInstallingState(string details, bool inProgress)
        {
            UpdateUIState("Installing...", details, 14, showProgressBar: true, showButtons: false);
        }

        private void HandleErrorState(string details, bool inProgress)
        {
            UpdateUIState("Installation error", details, 14, showProgressBar: false, showButtons: true);
        }

        private void UpdateUIState(string title, string details, double fontSize, bool showProgressBar, bool showButtons)
        {
            Title.Text = title;
            Title.IsVisible = !string.IsNullOrWhiteSpace(title);
            TextDetails.Text = details;
            TextDetails.FontSize = fontSize;
            ProgressBar.IsVisible = showProgressBar;
            Buttons.IsVisible = showButtons;
        }

        private Task<bool> WaitForUserConfirmation(string message)
        {
            var tcs = new TaskCompletionSource<bool>();

            // Show the message and bind actions to Confirm and Cancel buttons
            SetInstallationState(EnumState.Waiting, message);

            Confirm.Click += (sender, e) =>
            {
                tcs.TrySetResult(true);

            };

            Cancel.Click += (sender, e) =>
            {
                tcs.TrySetResult(false);
            };

            return tcs.Task;
        }

        public async Task StartVerification()
        {
            App.Main.Modals.Children.Add(this);
            SetInstallationState(EnumState.Checking);

            if (AppsListToInstall.Count == 0)
            {
                await ShowMessageAndClose("Select at least one app to install.");
                return;
            }

            List<string> alreadyInstalledApps = new();

            for (int i = 0; i < AppsListToInstall.Count; i++)
            {
                var app = AppsListToInstall[i];
                TextDetails.Text = $"{app.Name} ({i + 1}/{AppsListToInstall.Count})";
                string result = await Command.Execute("cmd.exe", $"/c {Command.wingetExe} list -q {app.WingetId} --accept-source-agreements");

                if (result.Contains(app.WingetId))
                {
                    alreadyInstalledApps.Add(app.WingetId);
                }
            }

            if (AppsListToInstall.Count == alreadyInstalledApps.Count)
            {
                await ShowMessageAndClose("All selected apps are already installed.");
                return;
            }

            if (alreadyInstalledApps.Count > 0)
            {
                string message = $"{alreadyInstalledApps.Count} app{(alreadyInstalledApps.Count > 1 ? "s" : "")} already installed, {AppsListToInstall.Count - alreadyInstalledApps.Count} to go. \nContinue the installation?";
                if (!await WaitForUserConfirmation(message)) return;
            }

            var appsToInstall = AppsListToInstall.FindAll(app => !alreadyInstalledApps.Contains(app.WingetId));
            await StartInstallation(appsToInstall);
        }

        private async Task StartInstallation(List<Footer.AppToInstall> apps)
        {
            SetInstallationState(EnumState.Installing);

            try
            {
                for (int i = 0; i < apps.Count; i++)
                {
                    var app = apps[i];
                    SetInstallationState(EnumState.Installing, $"{app.Name} ({i + 1}/{apps.Count})", inProgress: true);

                    string result = await Command.Execute("cmd.exe", $"/c {Command.wingetExe} install {app.WingetId} --accept-source-agreements --accept-package-agreements");
                    ProgressBar.Value = ((i + 1) * 100) / apps.Count;

                    if (!result.Contains(app.WingetId))
                    {
                        throw new Exception($"Error installing {app.Name}.");
                    }
                }

                await ShowMessageAndClose("All apps were successfully installed!");
            }
            catch (Exception ex)
            {
                SetInstallationState(EnumState.Error, ex.Message);
            }
        }

        private async Task ShowMessageAndClose(string message)
        {
            if (await WaitForUserConfirmation(message))
            {
                App.Main.Modals.Children.Clear();
            }
        }

        private void CloseButton_OnClick(object? sender, RoutedEventArgs e)
        {
            Command.Execute("cmd.exe", "/c powershell Stop-Process -Name 'winget' -Force");
            App.Main.Modals.Children.Clear();
        }
    }
}
