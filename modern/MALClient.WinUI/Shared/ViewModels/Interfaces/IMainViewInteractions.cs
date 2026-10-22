using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;

namespace MALClient.UWP.Shared.ViewModels.Interfaces
{
    public interface IMainViewInteractions
    {
        Storyboard CurrentStatusStoryboard { get; }
        Storyboard CurrentOffStatusStoryboard { get; }
        Storyboard CurrentOffSubStatusStoryboard { get; }
        SplitViewDisplayMode CurrentDisplayMode { get; }
        void SearchInputFocus(FocusState state);
        void InitSplitter();
    }
}
