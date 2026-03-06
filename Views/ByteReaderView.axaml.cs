using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;

namespace JpegVisualDecoder.Views;

public partial class ByteReaderView : UserControl
{
    public ByteReaderView()
    {
        InitializeComponent();

        Loaded += (_, _) =>
        {
            var vm = (ByteReaderViewModel)DataContext!;
            vm.PropertyChanged += async (_, e) =>
            {
                if (e.PropertyName == nameof(vm.PositionInChunk))
                    Dispatcher.UIThread.Post(() => Grid.ContainerFromIndex(vm.PositionInChunk)?.SetValue(TextBlock.BackgroundProperty, Brushes.AliceBlue));
            };
        };
    }
}