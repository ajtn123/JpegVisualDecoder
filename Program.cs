global using Avalonia;
global using JpegVisualDecoder.ViewModels;
global using JpegVisualDecoder.Views;

namespace JpegVisualDecoder;

internal sealed class Program
{
    [STAThread]
    public static void Main(string[] args)
        => BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
}
