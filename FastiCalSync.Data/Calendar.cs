using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace FastiCalSync.Data
{
    public enum SyncState : int
    {
        Syncing = 1,
        Paused = 2,
        Deleting = 3
    }

    public class Calendar : TableEntity
    {
        public Calendar(string userName, string iCalendarUrl)
        {
            this.UserName = userName;
            ID = Guid.NewGuid().ToString();
            this.iCalendarUrl = iCalendarUrl;
            SyncState = SyncState.Syncing;
        }

        /// <summary>
        /// For internal use only.
        /// </summary>
        public Calendar()
        { }

        public string UserName
        {
            get => PartitionKey;
            set => PartitionKey = value;
        }

        public string ID
        {
            get => RowKey;
            set => RowKey = value;
        }

        public string iCalendarUrl { get; set; }
        public string GoogleCalendarID { get; set; }
        public int InternalSyncState { get; set; }

        // Enums and bytes aren't supported by Azure Table Storage.
        public SyncState SyncState
        {
            get => (SyncState)InternalSyncState;
            set => InternalSyncState = (int)value;
        }

        public DateTime CreatedTimestampUtc { get; set; } = DateTime.UtcNow;
        public DateTime? LastPauseTimestampUtc { get; set; }
        public DateTime? LastProcessTimestampUtc { get; set; }
        public DateTime? LastErrorTimestampUtc { get; set; }
        public string LastErrorMessage { get; set; }
        public string LastErrorStackTrace { get; set; }
        public int? JobRetryCount { get; set; }
        public DateTime? DontRetryJobUntilTimeUtc { get; set; }

        #region Sync count properties

        public int? LastSyncSourceEventCount { get; set; }

        public int? LastSyncCreateCount { get; set; }
        public int? LastSyncUpdateCount { get; set; }
        public int? LastSyncDeleteCount { get; set; }
        public int? LastSyncOperationCount
            => LastSyncCreateCount
            + LastSyncUpdateCount
            + LastSyncDeleteCount;

        public int? LastSyncRemainingCreateCount { get; set; }
        public int? LastSyncRemainingUpdateCount { get; set; }
        public int? LastSyncRemainingDeleteCount { get; set; }
        public int? LastSyncRemainingOperationCount
            => LastSyncRemainingCreateCount
            + LastSyncRemainingUpdateCount
            + LastSyncRemainingDeleteCount;

        public int TotalSyncCreateCount { get; set; }
        public int TotalSyncUpdateCount { get; set; }
        public int TotalSyncDeleteCount { get; set; }
        public int TotalSyncOperationCount
            => TotalSyncCreateCount
            + TotalSyncUpdateCount
            + TotalSyncDeleteCount;

        #endregion

        public CalendarSyncCountTracker BeginSyncCountTracking(
            int remainingCreateCount,
            int remainingUpdateCount,
            int remainingDeleteCount)
            => new CalendarSyncCountTracker(this,
                remainingCreateCount,
                remainingUpdateCount,
                remainingDeleteCount);

        public void Sync()
        {
            SyncState = SyncState.Syncing;
            JobRetryCount = null;
            DontRetryJobUntilTimeUtc = null;
        }

        public void Delete()
        {
            SyncState = SyncState.Deleting;
            JobRetryCount = null;
            DontRetryJobUntilTimeUtc = null;
        }

        public void Pause()
        {
            LastPauseTimestampUtc = DateTime.UtcNow;
            SyncState = SyncState.Paused;
        }

        public void RecordJobSuccess()
        {
            LastProcessTimestampUtc = DateTime.UtcNow;
            JobRetryCount = null;
            DontRetryJobUntilTimeUtc = null;
        }

        public void RecordJobError(Exception ex)
        {
            LastErrorTimestampUtc = DateTime.UtcNow;
            LastErrorMessage = ex.Message;
            LastErrorStackTrace = ex.StackTrace;

            LastProcessTimestampUtc = DateTime.UtcNow;

            JobRetryCount = (JobRetryCount + 1) ?? 0;
            DontRetryJobUntilTimeUtc = DateTime.UtcNow
                + TimeSpan.FromMinutes(Math.Pow(2, JobRetryCount.Value));

            if (JobRetryCount >= 10)
            {
                Pause();
            }
        }
    }
}
