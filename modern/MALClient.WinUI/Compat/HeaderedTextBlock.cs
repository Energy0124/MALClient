using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MALClient.WinUI.Compat
{
    /// <summary>
    ///     Replacement for the UWP Community Toolkit HeaderedTextBlock (removed in CommunityToolkit 8).
    /// </summary>
    public class HeaderedTextBlock : StackPanel
    {
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register(nameof(Header), typeof(string), typeof(HeaderedTextBlock),
                new PropertyMetadata(null, OnTextChanged));

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(HeaderedTextBlock),
                new PropertyMetadata(null, OnTextChanged));

        private readonly TextBlock _header = new TextBlock {FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 0, 6, 0)};
        private readonly TextBlock _text = new TextBlock {TextWrapping = TextWrapping.Wrap};

        public HeaderedTextBlock()
        {
            Children.Add(_header);
            Children.Add(_text);
        }

        public string Header
        {
            get => (string) GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        public string Text
        {
            get => (string) GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (HeaderedTextBlock) d;
            control._header.Text = control.Header ?? string.Empty;
            control._text.Text = control.Text ?? string.Empty;
        }
    }
}
