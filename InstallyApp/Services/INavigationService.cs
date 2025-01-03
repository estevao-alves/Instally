using Avalonia.Controls;

public interface INavigationService
{
    void NavigateTo(UserControl page);
    void GoBack();
}