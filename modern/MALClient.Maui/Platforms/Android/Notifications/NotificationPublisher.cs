using Android.App;
using Android.Content;
using Android.Graphics;
using AndroidX.Core.App;
using MALClient.XShared.Utils;

namespace MALClient.Maui.Notifications
{
    /// <summary>
    ///     Channels and posting shared by the background checks.
    /// </summary>
    public static class NotificationPublisher
    {
        public const string LaunchArgsKey = "launchArgs";
        public const string MalChannel = "malclient_notifications";
        public const string AiringChannel = "malclient_airing";
        public const string MalNotificationTag = "MALClient";

        public static void EnsureChannels(Context context)
        {
            if (!OperatingSystem.IsAndroidVersionAtLeast(26))
                return;
            var manager = (NotificationManager) context.GetSystemService(Context.NotificationService);
            manager.CreateNotificationChannel(new NotificationChannel(MalChannel, "MyAnimeList notifications",
                NotificationImportance.Default) {Description = "Notifications from MyAnimeList.net"});
            manager.CreateNotificationChannel(new NotificationChannel(AiringChannel, "New episodes",
                NotificationImportance.Default) {Description = "Episodes of shows you follow that have just aired"});
        }

        /// <summary>
        ///     Background jobs can start the process without the UI, the signed in user has to be loaded then.
        /// </summary>
        public static void EnsureCredentials()
        {
            if (string.IsNullOrEmpty(Credentials.UserName))
                Credentials.Init();
        }

        /// <summary>
        ///     Android 13 needs a runtime grant before anything shows up.
        /// </summary>
        public static void RequestPermission()
        {
            if (!OperatingSystem.IsAndroidVersionAtLeast(33))
                return;
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    if (await Permissions.CheckStatusAsync<Permissions.PostNotifications>() != PermissionStatus.Granted)
                        await Permissions.RequestAsync<Permissions.PostNotifications>();
                }
                catch (Exception)
                {
                    //no activity to ask from, the next start will ask
                }
            });
        }

        /// <summary>
        ///     string.GetHashCode differs between processes, which would orphan notifications posted by a job.
        /// </summary>
        public static int StableId(string value)
        {
            unchecked
            {
                var hash = (int) 2166136261;
                foreach (var c in value)
                    hash = (hash ^ c) * 16777619;
                return hash;
            }
        }

        public static PendingIntent LaunchIntent(Context context, string launchArgs, int requestCode)
        {
            var intent = new Intent(context, typeof(MainActivity));
            intent.SetFlags(ActivityFlags.NewTask | ActivityFlags.SingleTop);
            intent.PutExtra(LaunchArgsKey, launchArgs);
            return PendingIntent.GetActivity(context, requestCode, intent,
                PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);
        }

        public static async Task<Bitmap> LoadBitmapAsync(string url)
        {
            if (string.IsNullOrEmpty(url))
                return null;
            try
            {
                using var client = new HttpClient {Timeout = TimeSpan.FromSeconds(15)};
                var bytes = await client.GetByteArrayAsync(url);
                return await BitmapFactory.DecodeByteArrayAsync(bytes, 0, bytes.Length);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static void Notify(Context context, string tag, int id, NotificationCompat.Builder builder)
        {
            EnsureChannels(context);
            try
            {
                NotificationManagerCompat.From(context).Notify(tag, id, builder.Build());
            }
            catch (Java.Lang.SecurityException)
            {
                //notifications permission not granted
            }
        }
    }
}
