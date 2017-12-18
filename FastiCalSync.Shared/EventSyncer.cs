using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GCal = Google.Apis.Calendar.v3.Data;

namespace FastiCalSync.Shared
{
    public class EventSyncer
    {
        private iCalService iCalService;
        private GCalService gCalService;

        public EventSyncer(
            iCalService iCalService,
            GCalService gCalService)
        {
            this.iCalService = iCalService;
            this.gCalService = gCalService;
        }

        public async Task<string> GetOrCreateGCalID(Uri iCalUrl)
        {
            var list = await gCalService.GetCalendarList();
            string calendarID = list.SingleOrDefault(c => c.Summary.Equals(iCalUrl.ToString()))?.Id;

            if (calendarID == null)
            {
                var calendar = new GCal.Calendar { Summary = iCalUrl.ToString() };
                calendar = await gCalService.Insert(calendar);
                calendarID = calendar.Id;
            }

            return calendarID;
        }

        public async Task<IReadOnlyCollection<IEventSyncOperation>> GetEventSyncOperations(
            string gCalID, Uri iCalUrl)
        {
            var iCalEvents = (await iCalService.GetCalendar(iCalUrl)).Events
                .ToDictionary(e => e.Uid, e => new iCalEvent(e));
            var gCalEvents = (await gCalService.GetEvents(gCalID))
                .ToDictionary(e => e.ICalUID, e => new GCalEvent(e));
            var operations = new List<IEventSyncOperation>();

            operations.AddRange(
                from @event in iCalEvents.Values
                where !gCalEvents.ContainsKey(@event.Uid)
                select new ImportEventOperation(@event, gCalID));

            operations.AddRange(
                from iCalEvent in iCalEvents.Values
                where gCalEvents.ContainsKey(iCalEvent.Uid)
                let gCalEvent = gCalEvents[iCalEvent.Uid]
                where iCalEvent.HasChanges(gCalEvent)
                select new UpdateEventOperation(iCalEvent, gCalEvent, gCalID));

            operations.AddRange(
                from @event in gCalEvents.Values
                where !iCalEvents.ContainsKey(@event.Uid)
                select new DeleteEventOperation(@event, gCalID));

            return operations.OrderBy(o => o.Priority).ToArray();
        }
    }
}
