namespace JpegVisualDecoder.Codec;

public class ByteReader(byte[] data)
{
    public readonly byte[] data = data;

    public int position = 0;

    public byte ReadByte() => data[position++];

    public byte[] ReadBytes(int length) => data[position..(position += length)];

    public int ReadWord() => (data[position++] << 8) + data[position++];

    public bool HitRestart { get; set; }
    public int RestartMarker { get; set; }


    public void ClearRestart()
    {
        HitRestart = false;
    }
}
