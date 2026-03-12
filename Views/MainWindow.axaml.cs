using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;

namespace JpegVisualDecoder.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            DataContext = new MainWindowViewModel((App.Current!.ApplicationLifetime as ClassicDesktopStyleApplicationLifetime)!.Args![0]);
        }
    }
}