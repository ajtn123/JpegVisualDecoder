using Avalonia.Controls;

namespace JpegVisualDecoder.Views;

public partial class CanvasView : UserControl
{
    public CanvasView()
    {
        InitializeComponent();

        Loaded += async (_, _) =>
        {
            var vm = (CanvasViewModel)DataContext!;
            PeriodicTimer timer = new(TimeSpan.FromMilliseconds(200));
            while (await timer.WaitForNextTickAsync() && !vm.frozen)
                Img.InvalidateVisual();

            Img.InvalidateVisual();
            timer.Dispose();
        };
    }
}