namespace JpegVisualDecoder.Codec;

public partial class Decoder(ByteReaderViewModel brvm, LoggerViewModel logger, CompositeCanvasViewModel canvas)
{
    //private readonly ByteReader reader = brvm.Reader;
    private readonly ByteReader br = brvm.Reader;
    private void Log(string category, object message) => logger.Log(br.position, category, message.ToString() ?? "Null");

    public async Task DefineMarkers()
    {
        while (br.position < br.data.Length)
        {
            if (brvm.ReadByte() == 0xFF)
            {
                var marker = brvm.ReadByte();
                if (marker == 0x00) continue;

                brvm.AlternateColor();

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
        br.position = imageDataStart;
        int maxH = components.Max(c => c.X);
        int maxV = components.Max(c => c.Y);
        compIndexMap = components.Select((c, i) => (c.Id, i)).ToDictionary(x => x.Id, x => x.i);

        int mcuCount = 0;
        for (int mcuY = 0; mcuY < height; mcuY += maxV * 8)
        {
            for (int mcuX = 0; mcuX < width; mcuX += maxH * 8)
            {
                brvm.AlternateColor();

                if (restartInterval > 0 && mcuCount > 0 && mcuCount % restartInterval == 0)
                {
                    if (br.HitRestart)
                    {
                        Array.Clear(previousDC);
                        brvm.ClearRestart();
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
                        int[] raw = DecodeBlock(brvm, sel);
                        blocks[i] = ComputeIDCT(raw);
                    }

                    mcuData[comp.Id] = blocks;
                }

                RenderMCU(mcuData, mcuX, mcuY, maxH, maxV);
                mcuCount++;

                await Task.Delay(10);
            }
        }

        br.position = br.data.Length - 1;

        canvas.Cb.frozen = true;
        canvas.Cr.frozen = true;
        canvas.Y.frozen = true;
        canvas.Final.frozen = true;
    }
}
