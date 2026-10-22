using System;
using System.Collections.Generic;
using System.Threading;
using MALClient.Models.Enums;

namespace MALClient.Adapters.Common
{
    /// <summary>
    ///     In-process periodic jobs, ported from the UWP ThreadPoolTimer implementation.
    /// </summary>
    public class ScheduledJobsManager : ISchdeuledJobsManger
    {
        private readonly object _syncLock = new object();
        private readonly Dictionary<ScheduledJob, (Timer Timer, int Recurrence)> _jobTimers =
            new Dictionary<ScheduledJob, (Timer, int)>();

        public void StartJob(ScheduledJob job, int reccurence, Action jobDefinition)
        {
            lock (_syncLock)
            {
                if (_jobTimers.TryGetValue(job, out var existing))
                {
                    if (existing.Recurrence == reccurence)
                        return;

                    existing.Timer.Dispose();
                    _jobTimers.Remove(job);
                }

                var period = TimeSpan.FromMinutes(reccurence);
                var timer = new Timer(state => jobDefinition(), null, period, period);
                _jobTimers.Add(job, (timer, reccurence));
            }
        }

        public void StopJob(ScheduledJob job)
        {
            lock (_syncLock)
            {
                if (_jobTimers.TryGetValue(job, out var existing))
                {
                    existing.Timer.Dispose();
                    _jobTimers.Remove(job);
                }
            }
        }
    }
}
