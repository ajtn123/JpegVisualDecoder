using JpegVisualDecoder.Codec;

namespace JpegVisualDecoder.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public MainWindowViewModel(string? path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException();

        Path = path;
        ByteReaderViewModel = new(new(File.ReadAllBytes(path)));
        decoder = new(ByteReaderViewModel, LoggerViewModel, CompositeCanvasViewModel);

        Task.Run(ST);
    }

    private readonly Decoder decoder;

    public async Task ST()
    {
        await decoder.DefineMarkers();
        await decoder.DecodePixels();
    }

    public string Path { get; set; } = "Unspecified";
    public ByteReaderViewModel ByteReaderViewModel { get; init; }
    public LoggerViewModel LoggerViewModel { get; init; } = new();
    public CompositeCanvasViewModel CompositeCanvasViewModel { get; init; } = new();
}
