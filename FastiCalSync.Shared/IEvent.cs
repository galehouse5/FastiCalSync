using System;

namespace FastiCalSync.Shared
{
    public interface IEvent
    {
        string Uid { get; set; }
        DateTime? StartDate { get; set; }
        DateTime? StartTimeUtc { get; set; }
        DateTime? EndDate { get; set; }
        DateTime? EndTimeUtc { get; set; }
        string Summary { get; set; }
        string Location { get; set; }
        string Description { get; set; }
    }

    public static class IEventExtensions
    {
        public static bool HasChanges(this IEvent @event, IEvent other)
            => !Equals(@event.Uid, other.Uid)
            || !Equals(@event.StartDate, other.StartDate)
            || !Equals(@event.StartTimeUtc, other.StartTimeUtc)
            || !Equals(@event.EndDate, other.EndDate)
            || !Equals(@event.EndTimeUtc, other.EndTimeUtc)
            || !Equals(@event.Summary, other.Summary)
            || !Equals(@event.Location, other.Location)
            || !Equals(@event.Description, other.Description);

        public static void To(this IEvent @event, IEvent other)
        {
            other.Uid = @event.Uid;
            other.StartDate = @event.StartDate;
            other.StartTimeUtc = @event.StartTimeUtc;
            other.EndDate = @event.EndDate;
            other.EndTimeUtc = @event.EndTimeUtc;
            other.Summary = @event.Summary;
            other.Location = @event.Location;
            other.Description = @event.Description;
        }

        // Sync current events before future or past events.
        public static double GetSyncPriority(this IEvent @event)
        {
            if (@event.StartTimeUtc.HasValue)
                return Math.Abs((DateTime.UtcNow - @event.StartTimeUtc.Value).TotalDays);

            if (@event.StartDate.HasValue)
                return Math.Abs((DateTime.UtcNow.Date - @event.StartDate.Value).TotalDays);

            throw new NotImplementedException();
        }
    }
}
