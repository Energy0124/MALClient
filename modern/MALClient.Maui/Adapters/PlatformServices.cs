using MALClient.Adapters;
using MALClient.Models.Enums;
using MALClient.XShared.Interfaces;
using MALClient.XShared.Utils;
using MALClient.XShared.ViewModels;

namespace MALClient.Maui.Adapters
{
    //small platform services on top of MAUI Essentials

    public class ClipboardProvider : IClipboardProvider
    {
        public void SetText(string text)
        {
            MainThread.BeginInvokeOnMainThread(async () => await Clipboard.Default.SetTextAsync(text));
        }
    }

    public class SystemControlsLauncherService : ISystemControlsLauncherService
    {
        public void LaunchUri(Uri uri)
        {
            MainThread.BeginInvokeOnMainThread(async () => await Launcher.Default.OpenAsync(uri));
        }
    }

    public class ConnectionInfoProvider : IConnectionInfoProvider
    {
        public void Init()
        {
            HasInternetConnection = Connectivity.Current.NetworkAccess == NetworkAccess.Internet;
            Connectivity.Current.ConnectivityChanged += (sender, args) =>
                HasInternetConnection = args.NetworkAccess == NetworkAccess.Internet;
        }

        public bool HasInternetConnection { get; set; } = true;
    }

    public class DispatcherAdapter : IDispatcherAdapter
    {
        public void Run(Action action)
        {
            MainThread.BeginInvokeOnMainThread(action);
        }
    }

    public class SnackbarProvider : ISnackbarProvider
    {
        public void ShowText(string text)
        {
            MainThread.BeginInvokeOnMainThread(() =>
                Android.Widget.Toast.MakeText(Android.App.Application.Context, text, Android.Widget.ToastLength.Short)?.Show());
        }
    }

    public class ShareProvider : IShareProvider
    {
        public void Share(string message)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
                await Microsoft.Maui.ApplicationModel.DataTransfer.Share.Default.RequestAsync(
                    new ShareTextRequest(message, "MALClient")));
        }
    }

    /// <summary>
    ///     Opens images in the browser for now, saving into the gallery needs the media store permissions flow.
    /// </summary>
    public class ImageDownloaderService : IImageDownloaderService
    {
        public void DownloadImage(string url, string suggestedFilename, bool animeConver)
        {
            if (url != null)
                ResourceLocator.SystemControlsLauncherService.LaunchUri(new Uri(url));
        }

        public void DownloadImageDefault(string url, string suggestedFilename, bool animeCover)
        {
            DownloadImage(url, suggestedFilename, animeCover);
        }
    }

    public class ChangeLogProvider : IChangeLogProvider
    {
        public bool NewChangelog { get; set; }

        public string CurrentVersion => AppInfo.Current.VersionString;

        public string DateWithVersion => $"{CurrentVersion} - .NET MAUI";

        public List<string> Changelog => new List<string>
        {
            "Rebuilt on .NET MAUI.",
        };
    }

    public class CssManager : CssManagerBase
    {
        protected override string AccentColour => "#2E51A2";
        protected override string AccentColourLight => "#5A77BD";
        protected override string AccentColourDark => "#1D3A80";
        //MAUI's WebView has no script to app bridge, height notifications are dropped
        protected override string NotifyFunction => "(function(){})";
        protected override string ShadowsDefinition => "0px 0px 34px 4px";
    }

    public class DialogsProvider : IDialogsProvider
    {
        public async void ShowScoreDialog(AnimeItemViewModel vm)
        {
            var choices = Enumerable.Range(1, 10).Reverse().Select(score => score.ToString()).ToArray();
            var choice = await Pages.HostPage.Current.DisplayActionSheetAsync("Your score", "Cancel", "Remove score", choices);
            if (choice == null || choice == "Cancel")
                return;
            vm.ChangeScore(choice == "Remove score" ? "0" : choice);
        }
    }
}
