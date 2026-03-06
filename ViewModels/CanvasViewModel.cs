using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace JpegVisualDecoder.ViewModels;

public partial class CanvasViewModel : ViewModelBase
{
    [ObservableProperty] public partial WriteableBitmap? Bitmap { get; set; }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (Bitmap != null)
            width = Convert.ToInt32(Bitmap.Size.Width);
    }

    private int width;
    public void SetPixel(int x, int y, byte r, byte g, byte b)
    {
        using var buffer = Bitmap!.Lock();

        Marshal.Copy([r, g, b, byte.MaxValue], 0, buffer.Address + 4 * ((width * y) + x), 4);
    }

    public bool finished;
}
