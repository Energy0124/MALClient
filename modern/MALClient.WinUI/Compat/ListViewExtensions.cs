using System.Linq;
using Microsoft.UI.Xaml.Controls;

namespace MALClient.WinUI.Compat
{
    /// <summary>
    ///     Replacement for the WinRTXamlToolkit ListViewBase helpers used by the ported pages.
    /// </summary>
    public static class ListViewExtensions
    {
        public static void ScrollToBottom(this ListViewBase listView)
        {
            var scrollViewer = listView.GetFirstDescendantOfType<ScrollViewer>();
            scrollViewer?.ChangeView(null, scrollViewer.ScrollableHeight, null, true);
        }

        public static int GetFirstVisibleIndex(this ListViewBase listView)
        {
            switch (listView.ItemsPanelRoot)
            {
                case ItemsStackPanel stackPanel:
                    return stackPanel.FirstVisibleIndex;
                case ItemsWrapGrid wrapGrid:
                    return wrapGrid.FirstVisibleIndex;
                default:
                    return listView.Items.Count > 0 ? 0 : -1;
            }
        }
    }
}
