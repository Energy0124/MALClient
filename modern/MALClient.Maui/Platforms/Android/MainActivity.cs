using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using MALClient.Maui.Notifications;
using MALClient.Models.Enums;
using MALClient.Models.Models.Notifications;
using MALClient.XShared.Comm.MagicalRawQueries;
using MALClient.XShared.Utils;
using MALClient.XShared.ViewModels;

namespace MALClient.Maui;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
	protected override void OnCreate(Bundle savedInstanceState)
	{
		//picked up by the first navigation once the window is up
		if (savedInstanceState == null)
			MainViewModelBase.InitDetailsFull = ParseLaunchArgs(Intent);
		base.OnCreate(savedInstanceState);
	}

	protected override void OnNewIntent(Intent intent)
	{
		base.OnNewIntent(intent);
		var navArgs = ParseLaunchArgs(intent);
		if (navArgs != null)
			MainThread.BeginInvokeOnMainThread(() => ViewModelLocator.GeneralMain.Navigate(navArgs.Item1, navArgs.Item2));
	}

	/// <summary>
	///     Notifications pass a MAL url, prefixed with "id~" when opening it should also mark the notification read.
	/// </summary>
	private static Tuple<PageIndex, object> ParseLaunchArgs(Intent intent)
	{
		var args = intent?.GetStringExtra(NotificationPublisher.LaunchArgsKey) ?? intent?.DataString;
		if (string.IsNullOrWhiteSpace(args))
			return null;

		var separator = args.IndexOf('~');
		if (separator != -1)
		{
			//on a cold start this runs before the app signs in
			NotificationPublisher.EnsureCredentials();
			_ = MalNotificationsQuery.MarkNotifiactionsAsRead(new MalNotification(args.Substring(0, separator)));
			args = args.Substring(separator + 1);
		}

		var navArgs = MalLinkParser.GetNavigationParametersForUrl(args);
		//no page for it in the app, the site has it
		if (navArgs == null && Uri.TryCreate(args, UriKind.Absolute, out var url) && url.Scheme.StartsWith("http"))
			ResourceLocator.SystemControlsLauncherService.LaunchUri(url);
		return navArgs;
	}
}
