using MALClient.Models.Enums;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace MALClient.WinUI.Compat
{
    /// <summary>
    ///     Replacement for FontAwesome.UWP's control. <see cref="FontAwesomeIcon" /> values are the FontAwesome 4.7
    ///     code points, rendered with the bundled font (SIL Open Font License).
    /// </summary>
    public sealed class FontAwesome : UserControl
    {
        private static readonly FontFamily FontAwesomeFamily =
            new FontFamily("ms-appx:///Assets/Fonts/FontAwesome.otf#FontAwesome");

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register(nameof(Icon), typeof(FontAwesomeIcon), typeof(FontAwesome),
                new PropertyMetadata(FontAwesomeIcon.None, OnIconChanged));

        private readonly TextBlock _glyph = new TextBlock
        {
            FontFamily = FontAwesomeFamily,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        public FontAwesome()
        {
            IsTabStop = false;
            Content = _glyph;
        }

        public FontAwesomeIcon Icon
        {
            get => (FontAwesomeIcon) GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        private static void OnIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var icon = (FontAwesomeIcon) e.NewValue;
            ((FontAwesome) d)._glyph.Text = icon == FontAwesomeIcon.None ? string.Empty : ((char) (int) icon).ToString();
        }
    }
}
