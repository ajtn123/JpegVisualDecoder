using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;

namespace JpegVisualDecoder.Views
{
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
            DataContext = new MainWindowViewModel((App.Current!.ApplicationLifetime as ClassicDesktopStyleApplicationLifetime)!.Args![0]);
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
    }
}