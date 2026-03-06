namespace JpegVisualDecoder.Codec;

public class ByteReader(byte[] data)
{
    public readonly byte[] data = data;

    public int position = 0;
    private int bitPosition = 8; // 0..7
    private int current;

    public byte ReadByte() => data[position++];

    public byte[] ReadBytes(int length) => data[position..(position += length)];

    public int ReadWord() => (data[position++] << 8) + data[position++];

    public bool HitRestart { get; private set; }
    public int RestartMarker { get; private set; }

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
        int b = data[position++];

        if (b == 0xFF)
        {
            int next = data[position++];

            // stuffed byte
            if (next == 0x00)
                return 0xFF;

            // restart marker
            if (next >= 0xD0 && next <= 0xD7)
            {
                HitRestart = true;
                RestartMarker = next;
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

    public void ClearRestart()
    {
        HitRestart = false;
    }
}
