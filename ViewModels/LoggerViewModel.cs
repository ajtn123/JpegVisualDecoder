using System.Collections.ObjectModel;

namespace JpegVisualDecoder.ViewModels;

public partial class LoggerViewModel : ViewModelBase
{
    public ObservableCollection<Log> Logs { get; } = [];

    public void Log(int position, string category, string message)
    {
        if (Logs.Count > 16) Logs.RemoveAt(16);

        Logs.Insert(0, new(position, category, message));
    }
}

public record Log(int Position, string Category, string Message);
