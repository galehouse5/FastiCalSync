using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace FastiCalSync.Data
{
    public enum SyncState : int
    {
        Syncing = 1,
        PausedByUser = 2,
        PausedByError = 3,
        Deleting = 4
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
        public DateTime? LastSyncTimestampUtc { get; set; }
        public int? LastSyncSourceEventCount { get; set; }
        public DateTime? LastSyncErrorTimestampUtc { get; set; }
        public string LastSyncErrorMessage { get; set; }
        public string LastSyncErrorStackTrace { get; set; }
        public int? LastSyncErrorRetryCount { get; set; }

        public int? LastSyncImportCount { get; set; }
        public int? LastSyncUpdateCount { get; set; }
        public int? LastSyncDeleteCount { get; set; }
        public int? LastSyncOperationCount
            => LastSyncImportCount
            + LastSyncUpdateCount
            + LastSyncDeleteCount;

        public int? LastSyncRemainingImportCount { get; set; }
        public int? LastSyncRemainingUpdateCount { get; set; }
        public int? LastSyncRemainingDeleteCount { get; set; }
        public int? LastSyncRemainingOperationCount
            => LastSyncRemainingImportCount
            + LastSyncRemainingUpdateCount
            + LastSyncRemainingDeleteCount;

        public int TotalSyncImportCount { get; set; }
        public int TotalSyncUpdateCount { get; set; }
        public int TotalSyncDeleteCount { get; set; }
        public int TotalSyncOperationCount
            => TotalSyncImportCount
            + TotalSyncUpdateCount
            + TotalSyncDeleteCount;
    }
}
