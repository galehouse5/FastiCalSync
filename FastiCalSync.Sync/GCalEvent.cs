using Google.Apis.Calendar.v3.Data;
using System;

namespace FastiCalSync.Sync
{
    public class GCalEvent : IEvent
    {
        public GCalEvent(Event innerEvent)
        {
            this.InnerEvent = innerEvent;
        }

        public GCalEvent()
            : this(new Event())
        { }

        public Event InnerEvent { get; private set; }

        public string Uid
        {
            get => InnerEvent.ICalUID;
            set => InnerEvent.ICalUID = value;
        }

        public DateTime? StartDate
        {
            get => InnerEvent.Start?.Date == null ? (DateTime?)null : DateTime.Parse(InnerEvent.Start.Date);
            set
            {
                InnerEvent.Start = InnerEvent.Start ?? new EventDateTime();
                InnerEvent.Start.Date = value == null ? null : $"{value:yyyy-MM-dd}";
            }
        }

        public DateTime? StartTimeUtc
        {
            get => InnerEvent.Start?.DateTimeRaw == null ? (DateTime?)null : DateTime.Parse(InnerEvent.Start.DateTimeRaw);
            set
            {
                InnerEvent.Start = InnerEvent.Start ?? new EventDateTime();
                InnerEvent.Start.DateTimeRaw = value == null ? null : $"{value:o}";
            }
        }

        public DateTime? EndDate
        {
            get => InnerEvent.End?.Date == null ? (DateTime?)null : DateTime.Parse(InnerEvent.End.Date);
            set
            {
                InnerEvent.End = InnerEvent.End ?? new EventDateTime();
                InnerEvent.End.Date = value == null ? null : $"{value:yyyy-MM-dd}";
            }
        }

        public DateTime? EndTimeUtc
        {
            get => InnerEvent.End?.DateTimeRaw == null ? (DateTime?)null : DateTime.Parse(InnerEvent.End.DateTimeRaw);
            set
            {
                InnerEvent.End = InnerEvent.End ?? new EventDateTime();
                InnerEvent.End.DateTimeRaw = value == null ? null : $"{value:o}";
            }
        }

        public string Summary
        {
            get => InnerEvent.Summary;
            set => InnerEvent.Summary = value;
        }

        public string Location
        {
            get => InnerEvent.Location;
            set => InnerEvent.Location = value;
        }

        public string Description
        {
            get => InnerEvent.Description;
            set => InnerEvent.Description = value;
        }
    }
}
