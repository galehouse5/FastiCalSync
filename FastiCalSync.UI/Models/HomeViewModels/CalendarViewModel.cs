using FastiCalSync.Data;
using System;

namespace FastiCalSync.UI.Models.HomeViewModels
{
    public class CalendarViewModel
    {
        public string RowKey { get; set; }
        public string LongUrl { get; set; }
        public string ShortUrl { get; set; }
        public int? SourceEventCount { get; set; }
        public int? OutOfSyncEventCount { get; set; }
        public SyncState SyncState { get; set; }
        public bool HasError { get; set; }
        public DateTime? RetryTimeUtc { get; set; }
        public TimeSpan? TimeUntilRetry { get; set; }
        public DateTime? LastProcessTimestampUtc { get; set; }
        public TimeSpan? TimeSinceLastProcess { get; set; }
        public bool CanPause { get; set; }
        public bool CanStart { get; set; }
        public bool CanDelete { get; set; }

        public static CalendarViewModel Create(Calendar calendar)
        {
            string longUrl = calendar.iCalendarUrl;
            string shortUrl = longUrl.Length > 50 ? longUrl.Substring(0, 49) + "…" : longUrl;

            return new CalendarViewModel
            {
                RowKey = calendar.RowKey,
                LongUrl = longUrl,
                ShortUrl = shortUrl,
                SourceEventCount = calendar.LastSyncSourceEventCount,
                OutOfSyncEventCount = calendar.LastSyncRemainingOperationCount,
                SyncState = calendar.SyncState,
                HasError = calendar.JobRetryCount.HasValue,
                RetryTimeUtc = calendar.SyncState == SyncState.Paused ? null
                    : calendar.DontRetryJobUntilTimeUtc,
                TimeUntilRetry = calendar.SyncState == SyncState.Paused ? null
                    : calendar.DontRetryJobUntilTimeUtc - DateTime.UtcNow,
                LastProcessTimestampUtc = calendar.LastProcessTimestampUtc,
                TimeSinceLastProcess = DateTime.UtcNow - calendar.LastProcessTimestampUtc,
                CanPause = calendar.SyncState == SyncState.Syncing,
                CanStart = calendar.SyncState == SyncState.Paused,
                CanDelete = calendar.SyncState != SyncState.Deleting
            };
        }
    }
}
