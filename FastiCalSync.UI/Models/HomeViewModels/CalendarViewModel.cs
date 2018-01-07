using FastiCalSync.Data;
using System;

namespace FastiCalSync.UI.Models.HomeViewModels
{
    public class CalendarViewModel
    {
        public string RowKey { get; set; }
        public string LongUrl { get; set; }
        public string ShortUrl { get; set; }
        public string PrimaryState { get; set; }
        public string SecondaryState { get; set; }
        public DateTime? LastSyncTimestampUtc { get; set; }
        public TimeSpan? LastSyncTimeAgo { get; set; }
        public bool CanPause { get; set; }
        public bool CanStart { get; set; }
        public bool CanDelete { get; set; }

        public static CalendarViewModel Create(Calendar metadata)
        {
            string GetPrimaryState()
            {
                if (metadata.SyncState == SyncState.Deleting)
                    return "Deleting";

                if (metadata.SyncState == SyncState.PausedByError
                    || metadata.SyncState == SyncState.PausedByUser)
                    return "Paused";

                if (metadata.SyncState == SyncState.Syncing)
                    return "Syncing";

                throw new NotSupportedException();
            }

            string GetSecondaryState()
            {
                if (metadata.SyncState == SyncState.Deleting)
                    return null;

                if (metadata.SyncState == SyncState.PausedByError)
                    return "Error";

                if (metadata.SyncState == SyncState.PausedByUser)
                    return null;

                if (metadata.SyncState == SyncState.Syncing)
                {
                    if (metadata.LastSyncRemainingOperationCount > 0)
                        return $"{metadata.LastSyncRemainingOperationCount:n0} events";

                    if (metadata.LastSyncRemainingOperationCount == 0)
                        return "Current";

                    return null;
                }

                throw new NotSupportedException();
            }

            string longUrl = metadata.iCalendarUrl;
            string shortUrl = longUrl.Length > 50 ? longUrl.Substring(0, 49) + "…" : longUrl;

            return new CalendarViewModel
            {
                RowKey = metadata.RowKey,
                LongUrl = longUrl,
                ShortUrl = shortUrl,
                PrimaryState = GetPrimaryState(),
                SecondaryState = GetSecondaryState(),
                LastSyncTimestampUtc = metadata.LastSyncTimestampUtc,
                LastSyncTimeAgo = DateTime.UtcNow - metadata.LastSyncTimestampUtc,
                CanPause = metadata.SyncState == SyncState.Syncing,
                CanStart = metadata.SyncState == SyncState.PausedByError
                    || metadata.SyncState == SyncState.PausedByUser,
                CanDelete = metadata.SyncState != SyncState.Deleting
            };
        }
    }
}
