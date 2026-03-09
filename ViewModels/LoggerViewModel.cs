using System.Collections.ObjectModel;

namespace JpegVisualDecoder.ViewModels;

public partial class LoggerViewModel : ViewModelBase
{
    public const int MaxLog = 256;

    public ObservableCollection<Log> Logs { get; } = [];

    public void Log(int position, string category, string message)
    {
        if (Logs.Count > MaxLog) Logs.RemoveAt(MaxLog);

        Logs.Insert(0, new(position, category, message));
    }
}

public record Log(int Position, string Category, string Message);
