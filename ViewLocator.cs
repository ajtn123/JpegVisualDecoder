using Avalonia.Controls;
using Avalonia.Controls.Templates;

namespace JpegVisualDecoder;

public class ViewLocator : IDataTemplate
{
    public Control? Build(object? param) => param switch
    {
        null => null,
        ByteReaderViewModel => new ByteReaderView(),
        LoggerViewModel => new LoggerView(),
        CanvasViewModel => new CanvasView(),
        _ => new TextBlock { Text = param.ToString() },
    };

    public bool Match(object? data) => data is ViewModelBase;
}
