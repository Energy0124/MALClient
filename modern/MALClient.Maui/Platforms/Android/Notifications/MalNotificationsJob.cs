using Android.App;
using Android.App.Job;
using Android.Content;
using AndroidX.Core.App;
using MALClient.Adapters;
using MALClient.Models.Enums;
using MALClient.Models.Models.Notifications;
using MALClient.XShared.Comm.Forums;
using MALClient.XShared.Comm.MagicalRawQueries;
using MALClient.XShared.Comm.MagicalRawQueries.Messages;
using MALClient.XShared.Utils;
using MALClient.XShared.ViewModels;

namespace MALClient.Maui.Notifications
{
    //stable java name, persisted jobs are looked up by it after updates and reboots
    [Service(Name = "com.energy0124.malclient.MalNotificationsJob", Exported = false,
        Permission = "android.permission.BIND_JOB_SERVICE")]
    public class MalNotificationsJob : JobService
    {
        public override bool OnStartJob(JobParameters @params)
        {
            Task.Run(async () =>
            {
                try
                {
                    await MalNotificationsChecker.RunAsync(ApplicationContext);
                }
                finally
                {
                    JobFinished(@params, false);
                }
            });
            return true;
        }

        public override bool OnStopJob(JobParameters @params)
        {
            return true;
        }
    }

    /// <summary>
    ///     Fetches unread MAL notifications, messages and watched topic replies and posts the ones not shown yet.
    /// </summary>
    public static class MalNotificationsChecker
    {
        private static readonly SemaphoreSlim Running = new SemaphoreSlim(1);

        public static async Task RunAsync(Context context)
        {
            //the in app timer and the job can overlap
            if (!await Running.WaitAsync(0))
                return;
            try
            {
                NotificationPublisher.EnsureCredentials();
                if (!Credentials.Authenticated)
                    return;

                var notifications = await FetchAsync();
                if (notifications.Count == 0)
                    return;

                var shown = (ResourceLocator.ApplicationDataService[nameof(RoamingDataTypes.ReadNotifications)] as
                    string ?? string.Empty).Split(';').ToList();
                foreach (var notification in notifications.Where(notification => !shown.Contains(notification.Id)))
                    await ShowAsync(context, notification);

                //only remember what is still unread so the list doesn't grow forever
                ResourceLocator.ApplicationDataService[nameof(RoamingDataTypes.ReadNotifications)] =
                    string.Join(";", notifications.Select(notification => notification.Id));
            }
            catch (Exception)
            {
                //try again on the next run
            }
            finally
            {
                Running.Release();
            }
        }

        private static async Task<List<MalNotification>> FetchAsync()
        {
            var enabled = Settings.EnabledNotificationTypes;
            var notifications = new List<MalNotification>();
            //everything MAL lists on its notifications page
            if ((enabled & ~(MalNotificationsTypes.Messages | MalNotificationsTypes.WatchedTopic)) != 0)
                try
                {
                    notifications.AddRange((await MalNotificationsQuery.GetNotifications()).Where(notification =>
                        !notification.IsRead && (enabled & notification.Type) == notification.Type));
                }
                catch (Exception)
                {
                    //page unavailable
                }

            if ((enabled & MalNotificationsTypes.Messages) == MalNotificationsTypes.Messages)
                try
                {
                    notifications.AddRange((await AccountMessagesManager.GetMessagesAsync(1))
                        .Where(message => !message.IsRead)
                        .Select(message => new MalNotification(message)));
                }
                catch (Exception)
                {
                    //no messages
                }

            var watchedTopics = ResourceLocator.HandyDataStorage.WatchedTopics;
            var topicsUpdated = false;
            foreach (var topic in watchedTopics.StoredItems.Where(topic => !topic.OnCooldown))
            {
                var count = await new ForumTopicMessageCountQuery(topic.Id).GetMessageCount(true);
                if (count == null || count <= topic.LastCheckedReplyCount)
                    continue;

                var notification = new MalNotification(topic);
                var diff = count - topic.LastCheckedReplyCount;
                notification.Content += diff == 1 ? " There is one new reply." : $" There are {diff} new replies.";
                notifications.Add(notification);
                topic.OnCooldown = true;
                topic.LastCheckedReplyCount = count.Value;
                topicsUpdated = true;
            }

            if (topicsUpdated)
                watchedTopics.SaveData();
            return notifications;
        }

        private static async Task ShowAsync(Context context, MalNotification notification)
        {
            var id = NotificationPublisher.StableId(notification.Id);
            var builder = new NotificationCompat.Builder(context, NotificationPublisher.MalChannel)
                .SetSmallIcon(Resource.Drawable.ic_stat_name)
                .SetContentTitle(notification.Header)
                .SetContentText(notification.Content)
                .SetStyle(new NotificationCompat.BigTextStyle().BigText(notification.Content))
                .SetAutoCancel(true)
                .SetGroup(notification.Type.GetDescription())
                .SetContentIntent(NotificationPublisher.LaunchIntent(context, notification.LaunchArgs, id));

            //messages and topic replies aren't MAL notifications, there is nothing to mark
            if (notification.Type != MalNotificationsTypes.Messages &&
                notification.Type != MalNotificationsTypes.WatchedTopic)
            {
                var readIntent = new Intent(context, typeof(MarkNotificationReadReceiver));
                readIntent.PutExtra(MarkNotificationReadReceiver.NotificationIdKey, notification.Id);
                builder.AddAction(Resource.Drawable.icon_eye_notification, "Mark as read",
                    PendingIntent.GetBroadcast(context, id, readIntent,
                        PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable));
            }

            NotificationPublisher.Notify(context, NotificationPublisher.MalNotificationTag, id, builder);
            //posting a burst at once gets some of them dropped
            await Task.Delay(500);
        }
    }

    [BroadcastReceiver(Name = "com.energy0124.malclient.MarkNotificationReadReceiver", Exported = false)]
    public class MarkNotificationReadReceiver : BroadcastReceiver
    {
        public const string NotificationIdKey = "NotificationMarkRead";

        public override void OnReceive(Context context, Intent intent)
        {
            var id = intent?.GetStringExtra(NotificationIdKey);
            if (id == null)
                return;

            NotificationManagerCompat.From(context)
                .Cancel(NotificationPublisher.MalNotificationTag, NotificationPublisher.StableId(id));
            var pending = GoAsync();
            Task.Run(async () =>
            {
                try
                {
                    NotificationPublisher.EnsureCredentials();
                    await MalNotificationsQuery.MarkNotifiactionsAsRead(new MalNotification(id));
                }
                catch (Exception)
                {
                    //stays unread on the site
                }
                finally
                {
                    pending.Finish();
                }
            });
        }
    }
}
