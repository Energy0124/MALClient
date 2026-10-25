using Android.App;
using Android.App.Job;
using Android.Content;
using AndroidX.Core.App;
using MALClient.Models.Enums;
using MALClient.Models.Models.Notifications;
using MALClient.XShared.ViewModels;

namespace MALClient.Maui.Notifications
{
    /// <summary>
    ///     Hourly check for new episodes of the shows registered from the details page.
    /// </summary>
    [Service(Name = "com.energy0124.malclient.AiredEpisodesJob", Exported = false,
        Permission = "android.permission.BIND_JOB_SERVICE")]
    public class AiredEpisodesJob : JobService
    {
        public override bool OnStartJob(JobParameters @params)
        {
            Task.Run(async () =>
            {
                try
                {
                    await RunAsync(ApplicationContext);
                }
                catch (Exception)
                {
                    //try again on the next run
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

        private static async Task RunAsync(Context context)
        {
            var entries = ResourceLocator.HandyDataStorage.RegisteredAiringNotifications;
            await ResourceLocator.AiringInfoProvider.Init(false);
            var expired = new List<AiringShowNotificationEntry>();
            var updated = false;
            foreach (var entry in entries.StoredItems.ToList())
            {
                var id = int.Parse(entry.Id);
                if (!ResourceLocator.AiringInfoProvider.TryGetCurrentEpisode(id, out var episode, DateTime.Today))
                {
                    expired.Add(entry);
                    continue;
                }

                if (episode == entry.TriggeredNotifications)
                    continue;

                var builder = new NotificationCompat.Builder(context, NotificationPublisher.AiringChannel)
                    .SetSmallIcon(Resource.Drawable.ic_stat_name)
                    .SetContentTitle("New anime episode is on air!")
                    .SetContentText($"Episode {episode} of {entry.Title} has just aired!")
                    .SetAutoCancel(true)
                    .SetGroup("airing")
                    .SetContentIntent(NotificationPublisher.LaunchIntent(context,
                        $"https://myanimelist.net/anime/{entry.Id}", NotificationPublisher.StableId(entry.Id)));
                var cover = await NotificationPublisher.LoadBitmapAsync(entry.ImageUrl);
                if (cover != null)
                    builder.SetLargeIcon(cover);

                entry.TriggeredNotifications = episode;
                //last episode, or nothing scheduled after this one
                if (entry.EpisodeCount == episode ||
                    !ResourceLocator.AiringInfoProvider.TryGetNextAirDate(id, DateTime.Today.AddDays(.9), out _))
                    expired.Add(entry);

                NotificationPublisher.Notify(context, null,
                    NotificationPublisher.StableId($"{entry.Id};ep{episode}"), builder);
                updated = true;
            }

            entries.StartBatch();
            foreach (var entry in expired)
                entries.StoredItems.Remove(entry);
            entries.CommitBatch();
            if (updated)
                entries.SaveData();

            if (entries.StoredItems.Count == 0)
                ResourceLocator.NotificationsTaskManager.StopTask(BgTasks.AiredNotification);
        }
    }
}
