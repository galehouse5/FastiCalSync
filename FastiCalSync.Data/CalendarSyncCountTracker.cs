using System;

namespace FastiCalSync.Data
{
    public class CalendarSyncCountTracker : IDisposable
    {
        private readonly Calendar calendar;
        private readonly int remainingCreateCount;
        private readonly int remainingUpdateCount;
        private readonly int remainingDeleteCount;

        protected internal CalendarSyncCountTracker(
            Calendar calendar,
            int remainingCreateCount,
            int remainingUpdateCount,
            int remainingDeleteCount)
        {
            this.calendar = calendar;
            this.remainingCreateCount = remainingCreateCount;
            this.remainingUpdateCount = remainingUpdateCount;
            this.remainingDeleteCount = remainingDeleteCount;
        }

        public int CreateCount { get; set; }
        public int UpdateCount { get; set; }
        public int DeleteCount { get; set; }

        public void Dispose()
        {
            calendar.LastSyncRemainingCreateCount = remainingCreateCount - CreateCount;
            calendar.LastSyncRemainingUpdateCount = remainingUpdateCount - UpdateCount;
            calendar.LastSyncRemainingDeleteCount = remainingDeleteCount - DeleteCount;

            calendar.LastSyncCreateCount = CreateCount;
            calendar.LastSyncUpdateCount = UpdateCount;
            calendar.LastSyncDeleteCount = DeleteCount;

            calendar.TotalSyncCreateCount += CreateCount;
            calendar.TotalSyncUpdateCount += UpdateCount;
            calendar.TotalSyncDeleteCount += DeleteCount;
        }
    }
}
