using Avalonia.Media;
using Avalonia.Threading;

namespace InstallyApp
{
    public partial class DebugStatus : Window
    {
        public DebugStatus()
        {
            InitializeComponent();
        }

        public void CreateInfo(string? result)
        {
            Dispatcher.UIThread.Post(() =>
            {
                // Append the new line to the existing content
                TextBlock debugTextBlock = new()
                {
                    TextWrapping = TextWrapping.Wrap,
                    Text = $"{result}"
                };

                // Add the new TextBlock to the DebugWrapper
                if (DebugWrapper.Content is Panel panel)
                {
                    panel.Children.Add(debugTextBlock);
                }
                else
                {
                    StackPanel container = new();
                    container.Children.Add(debugTextBlock);
                    DebugWrapper.Content = container;
                }

                DebugWrapper.ScrollToEnd();
            });
        }
    }
}