using MALClient.Maui.ViewModels;

namespace MALClient.Maui.Pages
{
    public partial class MenuPage : ContentPage
    {
        public MenuPage()
        {
            InitializeComponent();
            BindingContext = MauiViewModelLocator.Hamburger;
            VersionLabel.Text = $"Version {AppInfo.Current.VersionString}";
        }
    }
}
