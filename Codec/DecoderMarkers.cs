namespace JpegVisualDecoder.Codec;

public partial class Decoder
{
    private void StartOfImage()
    {
        Log("Marker", "Start of Image");
    }

    private void StartOfFrameBaseline()
    {
        Log("Marker", "Start of Frame (Baseline)");
        var length = brvm.ReadWord();
        Log("| Length", length);
        var payload = brvm.ReadBytes(length - 2);
        precision = payload[0];
        height = (payload[1] << 8) | payload[2];
        width = (payload[3] << 8) | payload[4];
        components = new Component[payload[5]];
        for (int i = 0; i < components.Length; i++)
        {
            var offset = 6 + (3 * i);
            var id = payload[offset];
            var x = payload[offset + 1] / 16;
            var y = payload[offset + 1] % 16;
            var table = payload[offset + 2];
            components[i] = new(id, x, y, table);
        }

        Log("| Precision", precision);
        Log("| Line Nb", height);
        Log("| Samples", width);
        Log("| Components", components.Length);
    }

    private void StartOfFrameProgressive()
    {
        Log("Marker", "Start of Frame (Progressive)");
        throw new NotImplementedException();
    }

    private void DefineHuffmanTable()
    {
        Log("Marker", "Define Huffman Table");
        var length = brvm.ReadWord();
        Log("| Length", length);
        var payload = brvm.ReadBytes(length - 2);
        var TcTh = payload[0];
        var Tc = TcTh >> 4;
        var Th = TcTh & 0x0F;
        Log("| Type", $"{Th switch { 0 => "Luminance", 1 => "Chrominance", _ => Th }} {Tc switch { 0 => "DC", 1 => "AC", _ => Tc }}");
        var Li = payload[1..17];

        var table = new Dictionary<(ushort, byte), byte>();
        ushort code = 0;
        var offset = 17;
        for (byte len = 1; len <= 16; len++)
        {
            byte L = Li[len - 1];
            code <<= 1;
            for (int i = 0; i < L; i++)
            {
                table.Add((code++, len), payload[offset++]);
            }
        }
        HuffmanTables[Th, Tc] = table;
    }

    private void DefineQuantizationTable()
    {
        Log("Marker", "Define Quantization Table");
        var length = brvm.ReadWord();
        Log("| Length", length);
        var payload = brvm.ReadBytes(length - 2);

        int i = 0;
        while (i < payload.Length)
        {
            var PqTq = payload[i++];
            int Tq = PqTq & 0x0F;
            Log("Type", $"{Tq switch { 0 => "Luminance", 1 => "Chrominance", _ => Tq }} {precision}-bit");
            var table = new byte[64];

            // Read table in natural order (no zigzag here)
            for (int x = 0; x < 64; x++)
            {
                table[x] = payload[i++];
            }

            QuantizationTables[Tq] = table;
        }
    }
    private void DefineRestartInterval()
    {
        Log("Marker", "Define Restart Interval");
        var length = brvm.ReadWord();
        Log("| Length", length);
        restartInterval = brvm.ReadWord();
        Log("| Restart Interval", restartInterval);
    }

    private void StartOfScan()
    {
        Log("Marker", "Start Of Scan");
        var length = brvm.ReadWord();
        imageDataStart = br.position + length - 2;
        Log("| Length", length);
        var payload = brvm.ReadBytes(length - 2);
        componentMap = new ComponentSelector[payload[0]];
        for (int i = 0; i < componentMap.Length; i++)
        {
            var componentMapOffset = 1 + (2 * i);
            var id = payload[componentMapOffset];
            var dc = payload[componentMapOffset + 1] / 16;
            var ac = payload[componentMapOffset + 1] % 16;
            componentMap[i] = new(components.First(x => x.Id == id), dc, ac);
        }

        var specOffset = 1 + (2 * componentMap.Length);
        spectralSelectorStart = payload[specOffset];
        spectralSelectorEnd = payload[specOffset + 1];
        successiveApprox = payload[specOffset + 2];
    }

    private void Restart(int n)
    {
        Log("Marker", $"Restart {n}");
        restarts.Add(br.position - 2);
    }

    private void Application(int n)
    {
        Log("Marker", $"Application {n}");
    }

    private void Comment()
    {
        Log("Marker", "Comment");
    }

    private void EndOfImage()
    {
        Log("Marker", "End of Image");

        imageDataEnd = br.position - 2;

        Log("Meta", $"Entropy date length: {imageDataEnd - imageDataStart} bytes");
        Log("Meta", $"Image width: {width} pixels");
        Log("Meta", $"Image height: {height} pixels");
    }
}
