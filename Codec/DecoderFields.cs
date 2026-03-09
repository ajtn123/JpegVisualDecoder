namespace JpegVisualDecoder.Codec;

public partial class Decoder
{
    private readonly Dictionary<(ushort, byte), byte>[,] HuffmanTables = new Dictionary<(ushort, byte), byte>[4, 2];
    private readonly byte[][] QuantizationTables = new byte[4][];

    private int precision = default;
    private int height = default;
    private int width = default;
    private Component[] components = null!;
    private ComponentSelector[] componentMap = null!;

    private int spectralSelectorStart = default;
    private int spectralSelectorEnd = default;

    private int successiveApprox = default;

    private int imageDataStart = default;
    private int imageDataEnd = default;

    private int restartInterval = default;
    private readonly List<int> restarts = [];

    private readonly double[,] CosineLUT = new double[8, 8];
    private bool IsLutInit = false;

    private int[] previousDC = [];
    private Dictionary<int, int> compIndexMap = [];

    public static readonly int[] ZigZag = [
         0,  1,  8, 16,  9,  2,  3, 10,
        17, 24, 32, 25, 18, 11,  4,  5,
        12, 19, 26, 33, 40, 48, 41, 34,
        27, 20, 13,  6,  7, 14, 21, 28,
        35, 42, 49, 57, 58, 50, 43, 36,
        29, 22, 15, 23, 30, 37, 44, 51,
        52, 59, 60, 53, 45, 38, 31, 39,
        46, 54, 55, 61, 62, 47, 56, 63,
    ];

    private record Component(int Id, int X, int Y, int Table);
    private record ComponentSelector(Component Component, int DC, int AC);
}
