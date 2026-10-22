using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MALClient.Models.Enums;
using MALClient.UWP;
using MALClient.UWP.Shared;
using MALClient.UWP.Shared.Managers;
using MALClient.UWP.Shared.ViewModels;
using MALClient.UWP.ViewModels;
using MALClient.XShared.BL;
using MALClient.XShared.Comm.Anime;
using MALClient.XShared.Comm.Manga;
using MALClient.XShared.NavArgs;
using MALClient.XShared.Utils;
using MALClient.XShared.Utils.Managers;
using MALClient.XShared.ViewModels;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Windows.AppLifecycle;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
using Windows.UI;
using DataCache = MALClient.XShared.Utils.DataCache;
using LaunchActivatedEventArgs = Microsoft.UI.Xaml.LaunchActivatedEventArgs;

namespace MALClient.WinUI
{
    public partial class App : Application
    {
        private bool _savingState;

        public static MainWindow MainWindow { get; private set; }

        public App()
        {
            UnhandledException += (sender, e) => LogCrash(e.Exception);
            TaskScheduler.UnobservedTaskException += (sender, e) => LogCrash(e.Exception);
            UWPViewModelLocator.RegisterDependencies();
            DesktopViewModelLocator.RegisterDependencies();
            ResourceLocator.TelemetryProvider.Init();
            //can only be set before the first window is created
            RequestedTheme = (ApplicationTheme) Settings.SelectedTheme;
            InitializeComponent();
        }

        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            ApplyStartupNavigation(AppInstance.GetCurrent().GetActivatedEventArgs());

            MainWindow = new MainWindow();
            MainWindow.Closed += MainWindowOnClosed;
            MainWindow.Frame.NavigationFailed += OnNavigationFailed;
            MainWindow.Frame.Navigate(typeof(MainPage));
            ApplyTitleBarColours();
            MainWindow.Activate();

            await InitializationRoutines.InitApp();
            ImageCache.PerformScheduledCacheCleanup();
            InitializationRoutines.InitPostUpdate();
            ApplicationData.Current.LocalSettings.Values["AppVersion"] = UWPUtilities.GetAppVersion();
        }

        /// <summary>
        ///     Deep links (malclient:// protocol or MAL urls passed as launch arguments) open the matching page on start.
        /// </summary>
        private static void ApplyStartupNavigation(AppActivationArguments activation)
        {
            string url = null;
            switch (activation.Kind)
            {
                case ExtendedActivationKind.Protocol when activation.Data is IProtocolActivatedEventArgs protocol:
                    url = "https:" + protocol.Uri.OriginalString.Replace("malclient://", "");
                    break;
                case ExtendedActivationKind.Launch when activation.Data is ILaunchActivatedEventArgs launch &&
                                                       !string.IsNullOrWhiteSpace(launch.Arguments):
                    url = launch.Arguments;
                    break;
            }

            if (url == null)
                return;

            var navArgs = MalLinkParser.GetNavigationParametersForUrl(url);
            if (navArgs != null)
                MainViewModelBase.InitDetailsFull = navArgs;
        }

        private void ApplyTitleBarColours()
        {
            var titleBar = MainWindow.AppWindow.TitleBar;
            var background = Settings.SelectedTheme == (int) ApplicationTheme.Dark
                ? Color.FromArgb(255, 41, 41, 41)
                : Colors.White;
            titleBar.BackgroundColor = titleBar.ButtonBackgroundColor =
                titleBar.InactiveBackgroundColor = titleBar.ButtonInactiveBackgroundColor = background;
        }

        private static void LogCrash(Exception exception)
        {
            try
            {
                File.AppendAllText(Path.Combine(ApplicationData.Current.LocalFolder.Path, "crash.log"),
                    $"[{DateTime.Now:O}] {exception}{Environment.NewLine}{Environment.NewLine}");
            }
            catch (Exception)
            {
                //nowhere to report it
            }
        }

        private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        ///     Desktop apps get no suspension event, persist what UWP saved on suspend when the window closes.
        /// </summary>
        private async void MainWindowOnClosed(object sender, WindowEventArgs args)
        {
            if (_savingState)
                return;

            args.Handled = true;
            _savingState = true;
            try
            {
                await SaveState();
            }
            finally
            {
                Exit();
            }
        }

        private static async Task SaveState()
        {
            if (Settings.IsCachingEnabled)
            {
                if (AnimeUpdateQuery.UpdatedSomething)
                    await DataCache.SaveDataForUser(Credentials.UserName,
                        ResourceLocator.AnimeLibraryDataStorage.AllLoadedAnimeItemAbstractions.Select(
                            abstraction => abstraction.EntryData), AnimeListWorkModes.Anime);
                if (MangaUpdateQuery.UpdatedSomething)
                    await DataCache.SaveDataForUser(Credentials.UserName,
                        ResourceLocator.AnimeLibraryDataStorage.AllLoadedMangaItemAbstractions.Select(
                            abstraction => abstraction.EntryData), AnimeListWorkModes.Manga);
            }

            await DataCache.SaveVolatileData();
            await DataCache.SaveHumMalIdDictionary();
            await ViewModelLocator.ForumsMain.SavePinnedTopics();
            await FavouritesManager.SaveData();
            await AnimeImageQuery.SaveData();
            await ResourceLocator.HandyDataStorage.SaveData();
            await ResourceLocator.EnglishTitlesProvider.SaveData();
        }
    }
}
