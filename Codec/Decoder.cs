namespace JpegVisualDecoder.Codec;

public partial class Decoder(ByteReaderViewModel brvm, LoggerViewModel logger, CompositeCanvasViewModel canvas)
{
    private readonly ByteReader reader = brvm.ByteReader;
    private void Log(string category, object message) => logger.Log(reader.position, category, message.ToString() ?? "Null");

    public async Task DefineMarkers()
    {
        while (reader.position < reader.data.Length)
        {
            if (reader.ReadByte() == 0xFF)
            {
                var marker = reader.ReadByte();
                if (marker == 0x00) continue;

                switch (marker)
                {
                    case 0xD8: StartOfImage(); break;
                    case 0xC0: StartOfFrameBaseline(); break;
                    case 0xC2: StartOfFrameProgressive(); break;
                    case 0xC4: DefineHuffmanTable(); break;
                    case 0xDB: DefineQuantizationTable(); break;
                    case 0xDD: DefineRestartInterval(); break;
                    case 0xDA: StartOfScan(); break;
                    case byte b when (b >= 0xD0 && b <= 0xD7): Restart(b % 8); break;
                    case byte b when (b >= 0xE0 && b <= 0xEF): Application(b % 16); break;
                    case 0xFE: Comment(); break;
                    case 0xD9: EndOfImage(); break;
                    default: Log("Marker", $"Unknown (0x{marker:X2})"); break;
                }

                brvm.Refresh();
                await Task.Delay(1000);
            }
        }
    }

    public async Task DecodePixels()
    {
        canvas.Cb = new(width, height);
        canvas.Cr = new(width, height);
        canvas.Y = new(width, height);
        canvas.Final = new(width, height);

        // imageData = reader.data[imageDataStart..imageDataEnd];
        previousDC = new int[components.Length];
        brvm.ByteReader.position = imageDataStart;
        int maxH = components.Max(c => c.X);
        int maxV = components.Max(c => c.Y);
        compIndexMap = components.Select((c, i) => (c.Id, i)).ToDictionary(x => x.Id, x => x.i);

        int mcuCount = 0;
        for (int mcuY = 0; mcuY < height; mcuY += maxV * 8)
        {
            for (int mcuX = 0; mcuX < width; mcuX += maxH * 8)
            {
                if (restartInterval > 0 && mcuCount > 0 && mcuCount % restartInterval == 0)
                {
                    if (brvm.ByteReader.HitRestart)
                    {
                        Array.Clear(previousDC);
                        brvm.ByteReader.ClearRestart();
                    }
                    else
                        throw new Exception("Expected restart marker but none found");
                }

                var mcuData = new Dictionary<int, double[][]>();

                foreach (var sel in componentMap)
                {
                    var comp = sel.Component;
                    var blocks = new double[comp.Y * comp.X][];

                    for (int i = 0; i < comp.Y * comp.X; i++)
                    {
                        int[] raw = DecodeBlock(brvm.ByteReader, sel);
                        blocks[i] = ComputeIDCT(raw);
                    }

                    mcuData[comp.Id] = blocks;
                }

                RenderMCU(mcuData, mcuX, mcuY, maxH, maxV);
                mcuCount++;

                brvm.Refresh(false);
                await Task.Delay(10);
            }
        }

        brvm.ByteReader.position = brvm.ByteReader.data.Length - 1;
        brvm.Refresh(true);

        canvas.Final.finished = true;
    }
}
