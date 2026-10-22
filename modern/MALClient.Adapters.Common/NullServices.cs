using System;
using System.Runtime.CompilerServices;
using MALClient.Models.Enums;
using MALClient.Models.Models.Library;
using MALClient.Models.Models.Notifications;
using MALClient.XShared.Interfaces;

namespace MALClient.Adapters.Common
{
    //services the old apps implemented on top of features the new ones don't (yet) have:
    //live tiles, appcenter/hockeyapp telemetry, calendar export, pinned tiles and background notifications

    public class NullNotificationsTaskManager : INotificationsTaskManager
    {
        public void StartTask(BgTasks task)
        {
        }

        public void StopTask(BgTasks task)
        {
        }

        public void CallTask(BgTasks task)
        {
        }
    }

    public class NullAiringNotificationsAdapter : IAiringNotificationsAdapter
    {
        public void ScheduleToast(AiringShowNotificationEntry entry)
        {
        }

        public void RemoveToasts(string id)
        {
        }

        public bool AreNotificationRegistered(string id)
        {
            return false;
        }
    }

    public class NullLiveTilesManager : ILiveTilesManager
    {
        public void UpdateTile(IAnimeData item)
        {
        }
    }

    public class NullTelemetryProvider : ITelemetryProvider
    {
        public void Init()
        {
        }

        public void TelemetryTrackEvent(TelemetryTrackedEvents @event)
        {
        }

        public void TelemetryTrackEvent(TelemetryTrackedEvents @event, params (string Key, string Param)[] args)
        {
        }

        public void TelemetryTrackNavigation(PageIndex page)
        {
        }

        public void TelemetryTrackNavigation(ForumsPageIndex page)
        {
        }

        public void LogEvent(string @event)
        {
        }

        public void TrackExceptionWithMessage(Exception e, string message)
        {
        }

        public void TrackException(Exception e, [CallerMemberName] string caller = null)
        {
        }

        public void TrackExceptionWithAttachment(Exception e, string attachment = null,
            [CallerMemberName] string caller = null)
        {
        }
    }

    public class NullCalendarExportProvider : ICalendarExportProvider
    {
        public void ExportToCalendar(object animeItemViewModel)
        {
        }
    }

    public class NullPinTileService : IPinTileService
    {
        public void Load(object data)
        {
        }
    }
}
