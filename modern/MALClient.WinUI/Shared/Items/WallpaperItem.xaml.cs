using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using MALClient.XShared.ViewModels;
using MALClient.XShared.ViewModels.Items;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace MALClient.UWP.Shared.Items
{
    public sealed partial class WallpaperItem : UserControl
    {
        private WallpaperItemViewModel ViewModel => DataContext as WallpaperItemViewModel;

        public WallpaperItem()
        {
            this.InitializeComponent();
        }

        private async void Bitmap_OnImageOpened(object sender, RoutedEventArgs e)
        {
            if (Image.ActualWidth > 0)
            {
                BoundingGrid.Width = Image.ActualWidth;
                DescriptionGrid.Width = Image.ActualWidth;
                DescriptionGrid.MinWidth = 0;
                BoundingGrid.MinWidth = 0;
            }
            if (ViewModel != null)
            {
                ViewModel.Resolution = $"{Bitmap.PixelWidth}x{Bitmap.PixelHeight}";
                if (ViewModel.IsBlurred)
                    SetObscured(true);
            }
            ResolutionGrid.Visibility = Visibility.Visible;
            ImgLoading.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        ///     Flagged wallpapers stay hidden until tapped, UWP blurred them with a toolkit animation.
        /// </summary>
        private void SetObscured(bool obscured)
        {
            Image.Opacity = obscured ? 0.05 : 1;
        }

        private void WallpaperOnRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            if(ViewModelLocator.Mobile)
                return;
            var grid = sender as FrameworkElement;
            FlyoutBase.GetAttachedFlyout(grid).ShowAt(grid);
        }

        private void Bitmap_OnDownloadProgress(object sender, DownloadProgressEventArgs e)
        {
            DlProgress.Text = $"{e.Progress}%";
        }

        private void WallpaperOnHolding(object sender, HoldingRoutedEventArgs e)
        {
            var grid = sender as FrameworkElement;
            FlyoutBase.GetAttachedFlyout(grid).ShowAt(ResolutionGrid);
        }

        private void FrameworkElement_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (ViewModelLocator.Mobile)
            {
                DescriptionGrid.MaxWidth = Image.ActualWidth;
                BoundingGrid.MaxWidth = Image.ActualWidth;
            }
        }

        private async void UIElement_OnTapped(object sender, TappedRoutedEventArgs args)
        {
            if (ViewModel.IsBlurred)
                SetObscured(false);
        }
    }
}
