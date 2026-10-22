using System.Linq;
using Windows.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using MALClient.WinUI.Compat;

namespace MALClient.UWP.Shared.Managers
{
    /// <summary>
    ///     Blurs what's behind an overlay. The UWP version used a Win2D gaussian blur over a composition backdrop
    ///     brush (or the host backdrop), WinUI 3's in-app acrylic gives the same effect without Win2D.
    /// </summary>
    public class BlurHelper
    {
        public BlurHelper(FrameworkElement element, bool hostBackdrop)
        {
            ApplyBlur(element);
        }

        public BlurHelper(CommandBar element)
        {
            if (element.GetDescendants().FirstOrDefault() is Panel background)
                ApplyBlur(background);
        }

        private static void ApplyBlur(FrameworkElement element)
        {
            var brush = new AcrylicBrush
            {
                TintColor = Color.FromArgb(0xff, 0, 0, 0),
                TintOpacity = 0.3,
                FallbackColor = Color.FromArgb(0x7f, 0, 0, 0)
            };

            switch (element)
            {
                case Panel panel:
                    panel.Background = brush;
                    break;
                case Control control:
                    control.Background = brush;
                    break;
                case Border border:
                    border.Background = brush;
                    break;
            }
        }
    }
}
