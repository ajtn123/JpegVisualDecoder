using Avalonia.Media;
using Avalonia.Media.Immutable;
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

    public ByteViewModel[] HexChunk { get; } = GetFilledArray<ByteViewModel>(SIZE);

    [ObservableProperty] public partial int Chunk { get; set; } = -1;


    private int position = -1;
    public void Refresh(bool pointer = true)
    {
        if (position == ByteReader.position) return;
        position = ByteReader.position;

        var positionInChunk = position % SIZE;

        var chunkIndex = position / SIZE;
        if (Chunk != chunkIndex)
        {
            Chunk = chunkIndex;

            var start = position - positionInChunk;
            for (int i = 0; i < SIZE; i++)
            {
                var cell = start + i;
                if (cell < ByteReader.data.Length)
                {
                    byte value = ByteReader.data[cell];
                    HexChunk[i].SetValue(value);
                    HexChunk[i].SetBackground(ByteViewModel.Color.Transparent);
                }
            }
        }

        if (pointer)
            HexChunk[positionInChunk].SetBackground(ByteViewModel.Color.Blue);
    }

    private static T[] GetFilledArray<T>(int size) where T : class, new()
    {
        var array = new T[size];
        for (int i = 0; i < size; i++)
            array[i] = new T();
        return array;
    }
}

public partial class ByteViewModel : ViewModelBase
{
    [ObservableProperty] public partial char High { get; private set; }
    [ObservableProperty] public partial char Low { get; private set; }

    public void SetValue(int value)
    {
        High = GetHex(value / 16);
        Low = GetHex(value % 16);

        static char GetHex(int b) => b switch
        {
            0 => '0',
            1 => '1',
            2 => '2',
            3 => '3',
            4 => '4',
            5 => '5',
            6 => '6',
            7 => '7',
            8 => '9',
            9 => '9',
            10 => 'A',
            11 => 'B',
            12 => 'C',
            13 => 'D',
            14 => 'E',
            15 => 'F',
            _ => throw new InvalidOperationException(),
        };
    }

    [ObservableProperty] public partial ImmutableSolidColorBrush Background { get; private set; }

    public void SetBackground(Color color) => Background = brushes[(int)color];

    public enum Color { Transparent = 0, Red = 1, Green = 2, Blue = 3 }
    private static readonly ImmutableSolidColorBrush[] brushes = [new(Colors.Transparent), new(Colors.LightPink), new(Colors.LightCyan), new(Colors.LightBlue)];
}