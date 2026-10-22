using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;

namespace MALClient.UWP.Shared.ViewModels.Interfaces
{
    public interface IAnimeListViewInteractions
    {
        Flyout FlyoutFilters { get; }
        MenuFlyout FlyoutSorting { get; }
        ScrollViewer IndefiniteScrollViewer { set; }
        Flyout FlyoutViews { get; }
        Task<ScrollViewer> GetIndefiniteScrollViewer();
        void FlyoutSeasonSelectionHide();
    }
}
