using MALClient.Models.Enums;
using MALClient.XShared.Utils;
using MALClient.XShared.ViewModels;

namespace MALClient.Maui.Views
{
    public partial class SettingsView : ContentView, INavigatedView
    {
        private bool _loading;

        public SettingsView()
        {
            InitializeComponent();
        }

        public void OnNavigatedTo(object args)
        {
            _loading = true;
            AccountLabel.Text = Credentials.Authenticated ? $"Signed in as {Credentials.UserName}" : "Not signed in";
            DarkThemeSwitch.IsToggled = Settings.SelectedTheme == 1;
            EnglishTitlesSwitch.IsToggled = Settings.PreferEnglishTitles;
            VersionLabel.Text = $"Version {AppInfo.Current.VersionString}";
            _loading = false;
        }

        public void OnNavigatedFrom()
        {
        }

        private void AccountOnClicked(object sender, EventArgs e)
        {
            ViewModelLocator.GeneralMain.Navigate(PageIndex.PageLogIn);
        }

        private void DarkThemeOnToggled(object sender, ToggledEventArgs e)
        {
            if (_loading)
                return;
            Settings.SelectedTheme = e.Value ? 1 : 0;
            Application.Current.UserAppTheme = e.Value ? AppTheme.Dark : AppTheme.Light;
        }

        private void EnglishTitlesOnToggled(object sender, ToggledEventArgs e)
        {
            if (!_loading)
                Settings.PreferEnglishTitles = e.Value;
        }

        private async void ClearCacheOnClicked(object sender, EventArgs e)
        {
            await ResourceLocator.DataCacheService.ClearApiRelatedCache();
            ResourceLocator.SnackbarProvider.ShowText("Cache cleared.");
        }
    }
}
