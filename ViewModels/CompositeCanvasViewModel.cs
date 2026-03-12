using CommunityToolkit.Mvvm.ComponentModel;

namespace JpegVisualDecoder.ViewModels;

public partial class CompositeCanvasViewModel : ViewModelBase
{
    [ObservableProperty] public partial CanvasViewModel? Cb { get; set; }
    [ObservableProperty] public partial CanvasViewModel? Cr { get; set; }
    [ObservableProperty] public partial CanvasViewModel? Y { get; set; }
    [ObservableProperty] public partial CanvasViewModel? Final { get; set; }
}
