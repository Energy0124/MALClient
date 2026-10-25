using Android.App.Job;
using Android.Content;
using MALClient.Adapters;
using MALClient.Models.Enums;
using MALClient.XShared.Utils;

namespace MALClient.Maui.Notifications
{
    /// <summary>
    ///     Periodic background checks on the JobScheduler, ported from the Xamarin app.
    /// </summary>
    public class NotificationsTaskManager : INotificationsTaskManager
    {
        private const int NotificationsJobId = 3;
        private const int AiredEpisodesJobId = 4;

        private static Context Context => Android.App.Application.Context;

        private static JobScheduler Scheduler =>
            (JobScheduler) Context.GetSystemService(Context.JobSchedulerService);

        public void StartTask(BgTasks task)
        {
            switch (task)
            {
                case BgTasks.Notifications:
                    if (!Settings.EnableNotifications || !Credentials.Authenticated)
                        return;
                    Schedule<MalNotificationsJob>(NotificationsJobId,
                        TimeSpan.FromMinutes(Settings.NotificationsRefreshTime));
                    break;
                case BgTasks.AiredNotification:
                    Schedule<AiredEpisodesJob>(AiredEpisodesJobId, TimeSpan.FromHours(1));
                    break;
            }
        }

        public void StopTask(BgTasks task)
        {
            switch (task)
            {
                case BgTasks.Notifications:
                    Scheduler.Cancel(NotificationsJobId);
                    break;
                case BgTasks.AiredNotification:
                    Scheduler.Cancel(AiredEpisodesJobId);
                    break;
            }
        }

        public void CallTask(BgTasks task)
        {
            //the in app check that runs every few minutes while the app is open
            if (task == BgTasks.Notifications)
                _ = MalNotificationsChecker.RunAsync(Context);
        }

        private static void Schedule<TJob>(int id, TimeSpan interval) where TJob : JobService
        {
            NotificationPublisher.RequestPermission();

            //rescheduling restarts the period, so leave a matching job alone
            var intervalMs = (long) interval.TotalMilliseconds;
            if (Scheduler.GetPendingJob(id)?.IntervalMillis == Math.Max(intervalMs, JobInfo.MinPeriodMillis))
                return;

            var component = new ComponentName(Context, Java.Lang.Class.FromType(typeof(TJob)));
            Scheduler.Schedule(new JobInfo.Builder(id, component)
                .SetRequiredNetworkType(NetworkType.Any)
                .SetPersisted(true)
                .SetPeriodic(intervalMs)
                .Build());
        }
    }
}
