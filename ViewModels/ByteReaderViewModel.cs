using Avalonia.Media;
using Avalonia.Media.Immutable;
using CommunityToolkit.Mvvm.ComponentModel;
using JpegVisualDecoder.Codec;

namespace JpegVisualDecoder.ViewModels;

public partial class ByteReaderViewModel(ByteReader reader) : ViewModelBase
{
    public const int COLUMNS = 32;
    public const int ROWS = 32;
    public const int SIZE = COLUMNS * ROWS;

    public int Columns { get; } = COLUMNS;
    public int Rows { get; } = ROWS;

    private ByteReader ByteReader { get; init; } = reader;

    public int ChunkTotal { get; set; } = reader.data.Length / SIZE;

    public ByteViewModel[] HexChunk { get; } = GetFilledArray<ByteViewModel>(SIZE);

    [ObservableProperty] public partial int Chunk { get; set; } = -1;


    public void Refresh()
    {
        var chunkIndex = ByteReader.position / SIZE;
        if (Chunk != chunkIndex)
        {
            Chunk = chunkIndex;

            var start = Chunk * SIZE;
            for (int i = 0; i < SIZE; i++)
            {
                var cell = start + i;
                if (cell < ByteReader.data.Length)
                {
                    byte value = ByteReader.data[cell];
                    HexChunk[i].SetValue(value);
                    HexChunk[i].SetBackground(ByteViewModel.Color.Transparent);
                }
                else HexChunk[i].ClearValue();
            }
        }
    }

    public void MarkRange(int start, int end)
    {
        Refresh();
        //if (Chunk < 0) return;

        var chunkStart = Chunk * SIZE;
        var chunkEnd = chunkStart + SIZE;
        start = int.Clamp(start, chunkStart, chunkEnd);
        end = int.Clamp(end, chunkStart, chunkEnd);

        if (end >= start)
            for (int i = start; i <= end; i++)
                HexChunk[i - chunkStart].SetBackground((ByteViewModel.Color)color);
    }

    private static T[] GetFilledArray<T>(int size) where T : class, new()
    {
        var array = new T[size];
        for (int i = 0; i < size; i++)
            array[i] = new T();
        return array;
    }

    public byte ReadByte()
    {
        MarkRange(ByteReader.position, ByteReader.position);
        return ByteReader.ReadByte();
    }

    public byte[] ReadBytes(int length)
    {
        MarkRange(ByteReader.position, ByteReader.position + length);
        return ByteReader.ReadBytes(length);
    }

    public int ReadWord()
    {
        MarkRange(ByteReader.position, ByteReader.position + 2);
        return ByteReader.ReadWord();
    }

    public void ClearRestart() => ByteReader.ClearRestart();

    public ByteReader Reader => ByteReader;


    private int bitPosition = 8; // 0..7
    private int current;

    public int GetBit()
    {
        if (bitPosition == 8)
        {
            current = ReadEntropyByte();
            bitPosition = 0;
        }

        int bit = (current >> (7 - bitPosition)) & 1;
        bitPosition++;
        return bit;
    }
    private int ReadEntropyByte()
    {
        MarkRange(ByteReader.position, ByteReader.position);

        int b = Reader.data[Reader.position++];

        if (b == 0xFF)
        {
            int next = Reader.data[Reader.position++];

            // stuffed byte
            if (next == 0x00)
                return 0xFF;

            // restart marker
            if (next >= 0xD0 && next <= 0xD7)
            {
                Reader.HitRestart = true;
                Reader.RestartMarker = next;
                bitPosition = 8; // force byte alignment
                return ReadEntropyByte(); // continue with next data byte
            }

            throw new Exception($"Marker {next:X2} inside entropy data");
        }

        return b;
    }

    public int GetBits(int n)
    {
        int v = 0;
        for (int i = 0; i < n; i++)
            v = (v << 1) | GetBit();
        return v;
    }

    private int color = 1;
    public void AlternateColor()
    {
        color += 1;
        if (color > 3)
            color = 1;
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

    public void ClearValue()
    {
        High = Low = ' ';
        SetBackground(Color.Transparent);
    }

    [ObservableProperty] public partial ImmutableSolidColorBrush Background { get; private set; }

    public void SetBackground(Color color) => Background = brushes[(int)color];

    public enum Color { Transparent = 0, Red = 1, Green = 2, Blue = 3 }
    private static readonly ImmutableSolidColorBrush[] brushes = [new(Colors.Transparent), new(Colors.LightPink), new(Colors.LightGreen), new(Colors.LightBlue)];
}