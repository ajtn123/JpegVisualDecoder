using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;

namespace JpegVisualDecoder.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        WindowState = WindowState.FullScreen;
    }

    private async void Button_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        do await Reload(); while (loop);
    }

    private async Task Reload()
    {
        var vm = VM;
        DataContext = new MainWindowViewModel(Path);
        VM?.CompositeCanvasViewModel.Cb?.Bitmap.Dispose();
        VM?.CompositeCanvasViewModel.Cr?.Bitmap.Dispose();
        VM?.CompositeCanvasViewModel.Y?.Bitmap.Dispose();
        VM?.CompositeCanvasViewModel.Final?.Bitmap.Dispose();
        await Task.Run(VM!.Start);
    }

    private void Button_Tapped_1(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        new AboutWindow().Show();
    }

    private MainWindowViewModel? VM => DataContext as MainWindowViewModel;

    private bool loop;
    private void ToggleButton_Tapped(object? sender, Avalonia.Input.TappedEventArgs e) => loop = !loop;

    private void Button_Tapped_2(object? sender, Avalonia.Input.TappedEventArgs e) => Close();

    /// <summary>
    /// IsCapable StyledProperty definition
    /// </summary>
    public static readonly StyledProperty<string> PathProperty =
        AvaloniaProperty.Register<MainWindow, string>(nameof(Path), (App.Current!.ApplicationLifetime as ClassicDesktopStyleApplicationLifetime)!.Args![0]);

    /// <summary>
    /// Gets or sets the IsCapable property. This StyledProperty 
    /// indicates ....
    /// </summary>
    public string Path
    {
        get => GetValue(PathProperty);
        set => SetValue(PathProperty, value);
    }
}