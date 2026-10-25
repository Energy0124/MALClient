using MALClient.Models.Enums;
using MALClient.Models.Models.Notifications;
using MALClient.XShared.Interfaces;
using MALClient.XShared.ViewModels;

namespace MALClient.Maui.Adapters
{
    /// <summary>
    ///     Keeps the shows to watch for new episodes, <see cref="Notifications.AiredEpisodesJob" /> does the checking.
    /// </summary>
    public class AiringNotificationsAdapter : IAiringNotificationsAdapter
    {
        public void ScheduleToast(AiringShowNotificationEntry entry)
        {
            //only announce episodes that air from now on
            if (ResourceLocator.AiringInfoProvider.TryGetCurrentEpisode(int.Parse(entry.Id), out var episode,
                    DateTime.Today))
                entry.TriggeredNotifications = episode;
            ResourceLocator.HandyDataStorage.RegisteredAiringNotifications.StoredItems.Add(entry);
            ResourceLocator.NotificationsTaskManager.StartTask(BgTasks.AiredNotification);
        }

        public void RemoveToasts(string id)
        {
            var entries = ResourceLocator.HandyDataStorage.RegisteredAiringNotifications.StoredItems;
            var entry = entries.FirstOrDefault(item => item.Id == id);
            if (entry != null)
                entries.Remove(entry);
            if (entries.Count == 0)
                ResourceLocator.NotificationsTaskManager.StopTask(BgTasks.AiredNotification);
        }

        public bool AreNotificationRegistered(string id)
        {
            return ResourceLocator.HandyDataStorage.RegisteredAiringNotifications.StoredItems
                .Any(entry => entry.Id == id);
        }
    }
}
