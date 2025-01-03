namespace InstallyApp.Views.Controls;

public class TextField : TextBox
{
    public static readonly StyledProperty<UserControl> ExtraButtonProperty = AvaloniaProperty.Register<TextField, UserControl>(
        nameof(ExtraButton));

    public UserControl ExtraButton
    {
        get => GetValue(ExtraButtonProperty);
        set => SetValue(ExtraButtonProperty, value);
    }
    
    public static readonly StyledProperty<string> TextSizeProperty = AvaloniaProperty.Register<TextField, string>(
        nameof(TextSize));

    public string TextSize
    {
        get => GetValue(TextSizeProperty);
        set => SetValue(TextSizeProperty, value);
    }
}