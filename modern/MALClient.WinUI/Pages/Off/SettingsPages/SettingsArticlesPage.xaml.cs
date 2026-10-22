using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MALClient.UWP.Shared.Managers;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace MALClient.UWP.Pages.Off.SettingsPages
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsArticlesPage : Page
    {
        public SettingsArticlesPage()
        {
            InitializeComponent();
        }

        private void ButtonPinArticlesOnClick(object sender, RoutedEventArgs e)
        {
            //live tiles aren't supported by WinUI 3 apps
        }

        private void ButtonPinNewsOnClick(object sender, RoutedEventArgs e)
        {
            //live tiles aren't supported by WinUI 3 apps
        }

    }
}