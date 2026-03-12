using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System.Runtime.InteropServices;

namespace JpegVisualDecoder.ViewModels;

public partial class CanvasViewModel(int width, int height) : ViewModelBase
{
    public WriteableBitmap Bitmap { get; } = new WriteableBitmap(new PixelSize(width, height), new Vector(96, 96), PixelFormat.Rgba8888, AlphaFormat.Opaque);

    public void SetPixel(int x, int y, byte l)
        => SetPixel(x, y, l, l, l);

    public void SetPixel(int x, int y, byte r, byte g, byte b)
    {
        using var buffer = Bitmap.Lock();

        Marshal.Copy([r, g, b, byte.MaxValue], 0, buffer.Address + 4 * ((width * y) + x), 4);
    }

    public bool frozen;
}
