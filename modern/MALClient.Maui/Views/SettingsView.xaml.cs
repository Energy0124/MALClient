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
            ViewModelSections.BindingContext = ViewModel;
            RefreshTimePicker.ItemsSource = RefreshTimes.Select(minutes => $"{minutes} minutes").ToList();
            RefreshTimePicker.SelectedIndex = Array.IndexOf(RefreshTimes, ViewModel.NotificationsRefreshTime);
            BuildNotificationTypes();
            _loading = false;
        }

        private static readonly int[] RefreshTimes = {15, 30, 45, 60, 120};

        private static SettingsViewModelBase ViewModel => ViewModelLocator.Settings;

        private void BuildNotificationTypes()
        {
            NotificationTypes.Children.Clear();
            //WatchedTopic marks replies found by the app's own topic watcher, which isn't filtered by type
            foreach (var type in Enum.GetValues<MalNotificationsTypes>().Where(type =>
                         type != MalNotificationsTypes.Generic && type != MalNotificationsTypes.WatchedTopic))
            {
                var toggle = new CheckBox {IsChecked = ViewModel.EnabledNotificationTypes.HasFlag(type)};
                toggle.CheckedChanged += (sender, e) =>
                {
                    if (e.Value)
                        ViewModel.EnabledNotificationTypes |= type;
                    else
                        ViewModel.EnabledNotificationTypes &= ~type;
                };
                var row = new Grid {ColumnDefinitions = {new ColumnDefinition(GridLength.Auto), new ColumnDefinition()}};
                row.Add(toggle);
                row.Add(new Label {Text = type.GetDescription(), VerticalOptions = LayoutOptions.Center}, 1);
                NotificationTypes.Children.Add(row);
            }
        }

        private void RefreshTimeOnSelectedIndexChanged(object sender, EventArgs e)
        {
            if (_loading || RefreshTimePicker.SelectedIndex < 0)
                return;
            ViewModel.NotificationsRefreshTime = RefreshTimes[RefreshTimePicker.SelectedIndex];
            //reschedules with the new interval
            ResourceLocator.NotificationsTaskManager.StartTask(BgTasks.Notifications);
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
