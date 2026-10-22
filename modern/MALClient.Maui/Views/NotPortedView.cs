using MALClient.Models.Enums;

namespace MALClient.Maui.Views
{
    /// <summary>
    ///     Placeholder for pages that haven't been ported from the Xamarin app yet.
    /// </summary>
    public class NotPortedView : ContentView, INavigatedView
    {
        private readonly Label _label = new Label
        {
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalOptions = LayoutOptions.Center,
            Margin = new Thickness(32)
        };

        public NotPortedView()
        {
            Content = _label;
        }

        public void OnNavigatedTo(object args)
        {
            var page = args is PageIndex index ? index.ToString().Replace("Page", "") : "This page";
            _label.Text = $"{page} hasn't been ported to the new app yet.";
        }

        public void OnNavigatedFrom()
        {
        }
    }
}
