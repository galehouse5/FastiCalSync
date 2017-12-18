using Ical.Net.CalendarComponents;
using System;

namespace FastiCalSync.Shared
{
    public class iCalEvent : IEvent
    {
        public iCalEvent(CalendarEvent innerEvent)
        {
            this.InnerEvent = innerEvent;
        }

        public CalendarEvent InnerEvent { get; private set; }

        public string Uid
        {
            get => InnerEvent.Uid;
            set => throw new NotSupportedException();
        }

        public DateTime? StartDate
        {
            get => InnerEvent.DtStart.HasDate ? InnerEvent.DtStart.Date : (DateTime?)null;
            set => throw new NotSupportedException();
        }

        public DateTime? StartTimeUtc
        {
            get => InnerEvent.DtStart.HasTime ? InnerEvent.DtStart.Value : (DateTime?)null;
            set => throw new NotSupportedException();
        }

        public DateTime? EndDate
        {
            get => InnerEvent.DtEnd.HasDate ? InnerEvent.DtEnd.Date : (DateTime?)null;
            set => throw new NotSupportedException();
        }

        public DateTime? EndTimeUtc
        {
            get => InnerEvent.DtEnd.HasTime ? InnerEvent.DtEnd.Value : (DateTime?)null;
            set => throw new NotSupportedException();
        }

        public string Summary
        {
            get => InnerEvent.Summary;
            set => throw new NotImplementedException();
        }

        public string Location
        {
            get => InnerEvent.Location;
            set => throw new NotSupportedException();
        }

        public string Description
        {
            get => InnerEvent.Description;
            set => throw new NotSupportedException();
        }
    }
}
