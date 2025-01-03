using Avalonia.Controls;
using InstallyApp.Models;

namespace InstallyApp.DataServices;

public class NavigationService : INavigationService
{
    private readonly MainWindow _mainWindow;
    private readonly Stack<UserControl> _navigationStack = new();
    private readonly INavigationService navigationService;
    public NavigationService(MainWindow mainWindow)
    {
        _mainWindow = mainWindow;
    }

    public void NavigateTo(UserControl page)
    {
        if (_mainWindow.Content is UserControl currentPage)
        {
            _navigationStack.Push(currentPage);
        }

        _mainWindow.Content = page;
    }

    public void GoBack()
    {
        if (_navigationStack.Count > 0)
        {
            var previousPage = _navigationStack.Pop();
            _mainWindow.Content = previousPage;
        }
        else
        {
            Debug.WriteLine("No pages in navigation stack to go back to.");
        }
    }
}
