using CommunityToolkit.Mvvm.ComponentModel;
using JpegVisualDecoder.Codec;

namespace JpegVisualDecoder.ViewModels;

public partial class ByteReaderViewModel(ByteReader reader) : ViewModelBase
{
    public const int COLUMNS = 16;
    public const int ROWS = 32;
    public const int SIZE = COLUMNS * ROWS;

    public int Columns { get; } = COLUMNS;
    public int Rows { get; } = ROWS;

    public ByteReader ByteReader { get; init; } = reader;

    public int ChunkTotal { get; set; } = reader.data.Length / SIZE + 1;

    [ObservableProperty] public partial string[] HexChunk { get; set; } = new string[0];
    [ObservableProperty] public partial int PositionInChunk { get; set; }
    [ObservableProperty] public partial int Chunk { get; set; } = -1;


    private int position = -1;
    public void Refresh()
    {
        if (position == ByteReader.position) return;
        position = ByteReader.position;

        PositionInChunk = position % SIZE;

        var chunkIndex = position / SIZE;
        if (Chunk == chunkIndex) return;
        Chunk = chunkIndex;

        var start = position - PositionInChunk;
        var hex = new string[SIZE];
        for (int i = 0; i < SIZE; i++)
        {
            var cell = start + i;
            if (cell < ByteReader.data.Length)
                hex[i] = ByteReader.data[cell].ToString("X2");
        }
        HexChunk = hex;
    }
}
