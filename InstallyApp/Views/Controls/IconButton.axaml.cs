using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Media.Immutable;

namespace InstallyApp.Views.Controls;

public class IconButton : Button
{
    public static readonly StyledProperty<IBrush> IconColorProperty = AvaloniaProperty.Register<IconButton, IBrush>(
        nameof(IconColor));

    public IBrush IconColor
    {
        get => GetValue(IconColorProperty);
        set => SetValue(IconColorProperty, value);
    }
    
    public static readonly StyledProperty<StreamGeometry> IconProperty = AvaloniaProperty.Register<IconButton, StreamGeometry>(
        nameof(Icon));

    public StreamGeometry Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }
}