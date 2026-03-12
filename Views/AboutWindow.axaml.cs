using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace JpegVisualDecoder.Views;

public partial class AboutWindow : Window
{
    public AboutWindow()
    {
        InitializeComponent();
    }

    private void UniformGrid_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var grid = (UniformGrid)sender!;
        for (int i = 0; i < 64; i++)
        {
            grid.Children.Add(new Border { Width = 24, Height = 24, Background = new SolidColorBrush((uint)Random.Shared.Next(0xFFFFFF + 1) + 0xFF000000) });
        }
    }
}