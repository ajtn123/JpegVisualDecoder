namespace JpegVisualDecoder.Codec;

public partial class Decoder
{
    private void InitCosineLUT()
    {
        for (int u = 0; u < 8; u++)
        {
            for (int x = 0; x < 8; x++)
            {
                // Normalization factor (C(u))
                double cu = (u == 0) ? 1.0 / Math.Sqrt(2) : 1.0;
                // The cosine term: C(u) * cos(...)
                CosineLUT[u, x] = cu * Math.Cos((2 * x + 1) * u * Math.PI / 16.0);
            }
        }
        IsLutInit = true;
    }

    private double[] ComputeIDCT(int[] block)
    {
        if (!IsLutInit) InitCosineLUT();

        double[] output = new double[64];
        double[] temp = new double[64];

        // --- Pass 1: Rows (1D IDCT on rows) ---
        for (int i = 0; i < 8; i++)
        {
            for (int x = 0; x < 8; x++)
            {
                double sum = 0;
                for (int u = 0; u < 8; u++)
                {
                    // block[i*8 + u] is the frequency coeff (u is column index)
                    sum += block[i * 8 + u] * CosineLUT[u, x];
                }
                // Standard IDCT scaling allows factor of 1/2 per pass (total 1/4)
                temp[i * 8 + x] = sum / 2.0;
            }
        }

        // --- Pass 2: Columns (1D IDCT on columns) ---
        for (int x = 0; x < 8; x++) // Iterate columns of the temp block
        {
            for (int y = 0; y < 8; y++) // Iterate rows (output spatial y)
            {
                double sum = 0;
                for (int v = 0; v < 8; v++) // Iterate frequency v
                {
                    // Input is temp[v][x] (v is row index in temp)
                    sum += temp[v * 8 + x] * CosineLUT[v, y];
                }
                // Second 1/2 factor (total 1/4 matches textbook)
                double val = sum / 2.0;

                // Level Shift (+128) immediately
                output[y * 8 + x] = val + 128;
            }
        }

        return output;
    }

    private byte DecodeHuffman(ByteReader reader, Dictionary<(ushort, byte), byte> table)
    {
        ushort code = 0;
        for (byte length = 1; length <= 16; length++)
        {
            code = (ushort)((code << 1) | reader.GetBit());
            if (table.TryGetValue((code, length), out var symbol))
                return symbol;
        }
        throw new Exception("Invalid Huffman code");
    }

    private int ReceiveValue(ByteReader reader, int length)
    {
        if (length == 0) return 0;
        int val = reader.GetBits(length);
        // JPEG uses a specific encoding for negative numbers:
        // If the first bit is 0, it's negative.
        if (val < (1 << (length - 1)))
            val -= (1 << length) - 1;
        return val;
    }

    private int[] DecodeBlock(ByteReader reader, ComponentSelector selector)
    {
        int[] block = new int[64];

        // 1. Decode DC
        var dcTable = HuffmanTables[selector.DC, 0];
        int dcCategory = DecodeHuffman(reader, dcTable);
        int dcValue = ReceiveValue(reader, dcCategory);

        int compIndex = compIndexMap[selector.Component.Id];
        previousDC[compIndex] += dcValue;
        block[0] = previousDC[compIndex];

        // 2. Decode AC
        var acTable = HuffmanTables[selector.AC, 1];
        int k = 1;
        while (k < 64)
        {
            int symbol = DecodeHuffman(reader, acTable);
            if (symbol == 0x00) break; // EOB

            int numZeros = symbol >> 4;
            int acCategory = symbol & 0x0F;

            k += numZeros;
            if (k < 64)
            {
                block[k] = ReceiveValue(reader, acCategory);
                k++;
            }
        }

        // 3. De-Zig-Zag and Dequantize
        // We must put the k-th element from the bitstream into the ZigZag[k] position
        int[] result = new int[64];
        var qTable = QuantizationTables[selector.Component.Table];

        for (int i = 0; i < 64; i++)
        {
            // i is the ZigZag index (0..63)
            // ZigZag[i] is the Raster/Linear index (0..63)
            // qTable is stored in ZigZag order
            // block is stored in ZigZag order

            result[ZigZag[i]] = block[i] * qTable[i];
        }
        return result;
    }

    // Example for one pixel:
    private void YCbCrToRGB(double y, double cb, double cr, out byte r, out byte g, out byte b)
    {
        r = Clamp((int)(y + 1.402 * (cr - 128)));
        g = Clamp((int)(y - 0.344136 * (cb - 128) - 0.714136 * (cr - 128)));
        b = Clamp((int)(y + 1.772 * (cb - 128)));
    }

    private static byte Clamp(int val) => (byte)Math.Max(0, Math.Min(255, val));

    private void RenderMCU(Dictionary<int, double[][]> mcuData, int mcuX, int mcuY, int maxH, int maxV)
    {
        for (int py = 0; py < maxV * 8; py++)
        {
            for (int px = 0; px < maxH * 8; px++)
            {
                // Get Y value (Component ID 1)
                double y = GetPixelFromMCU(mcuData[1], px, py,
                    components.First(c => c.Id == 1).X,
                    components.First(c => c.Id == 1).Y, maxH, maxV);

                // Get Cb and Cr (Component IDs 2 and 3)
                double cb = GetPixelFromMCU(mcuData[2], px, py,
                    components.First(c => c.Id == 2).X,
                    components.First(c => c.Id == 2).Y, maxH, maxV);
                double cr = GetPixelFromMCU(mcuData[3], px, py,
                    components.First(c => c.Id == 3).X,
                    components.First(c => c.Id == 3).Y, maxH, maxV);

                YCbCrToRGB(y, cb, cr, out byte r, out byte g, out byte b);

                // Fix coordinates: px maps to X (width), py maps to Y (height)
                int xx = mcuX + px;
                int yy = mcuY + py;
                if (xx < width && yy < height)
                    canvas.SetPixel(xx, yy, r, g, b);
            }
        }
    }

    private double GetPixelFromMCU(double[][] blocks, int px, int py, int hFact, int vFact, int maxH, int maxV)
    {
        // For subsampled components, we need to scale down the pixel coordinates
        // Example: if maxH=2, hFact=1, then we read every other pixel
        int scaledX = (px * hFact) / maxH;
        int scaledY = (py * vFact) / maxV;

        // Which block within this component's blocks array?
        int blockX = scaledX / 8;
        int blockY = scaledY / 8;
        int blockIdx = blockY * hFact + blockX;

        // Which pixel within that 8x8 block?
        int innerX = scaledX % 8;
        int innerY = scaledY % 8;

        return blocks[blockIdx][innerY * 8 + innerX];
    }
}
