using MALClient.Maui.ViewModels;

namespace MALClient.Maui.Pages
{
    /// <summary>
    ///     Flyout (hamburger menu) around the single navigation host, like the Xamarin app's drawer.
    /// </summary>
    public class RootPage : FlyoutPage
    {
        public RootPage()
        {
            Flyout = new MenuPage();
            Detail = new HostPage(() => IsPresented = !IsPresented);
            FlyoutLayoutBehavior = FlyoutLayoutBehavior.Popover;
            MauiViewModelLocator.Hamburger.NavigationRequested += (sender, args) => IsPresented = false;
        }
    }
}
