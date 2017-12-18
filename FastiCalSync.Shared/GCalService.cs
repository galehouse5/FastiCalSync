using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FastiCalSync.Shared
{
    public class GCalService
    {
        private CalendarService service;

        public GCalService(UserCredential credential)
        {
            service = new CalendarService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "Fast iCal Sync"
            });
        }

        public async Task<IReadOnlyCollection<CalendarListEntry>> GetCalendarList()
        {
            var entries = new List<CalendarListEntry>();
            string nextPageToken = null;

            do
            {
                var request = service.CalendarList.List();
                request.PageToken = nextPageToken;
                request.MaxResults = 250; // "The page size can never be larger than 250 entries."

                var response = await request.ExecuteAsync();
                entries.AddRange(response.Items);

                nextPageToken = response.NextPageToken;
            }
            while (nextPageToken != null);

            return entries;
        }

        public async Task<Calendar> Insert(Calendar calendar)
        {
            var request = service.Calendars.Insert(calendar);
            return await request.ExecuteAsync();
        }

        public async Task<IReadOnlyCollection<Event>> GetEvents(string calendarID)
        {
            var events = new List<Event>();
            string nextPageToken = null;

            do
            {
                var request = service.Events.List(calendarID);
                request.PageToken = nextPageToken;
                request.MaxResults = 2500; // "The page size can never be larger than 2500 events."

                var response = await request.ExecuteAsync();
                events.AddRange(response.Items);

                nextPageToken = response.NextPageToken;
            }
            while (nextPageToken != null);

            return events;
        }

        public async Task<Event> Import(Event @event, string calendarID)
        {
            var request = service.Events.Import(@event, calendarID);
            return await request.ExecuteAsync();
        }

        public async Task<Event> Update(Event @event, string calendarID)
        {
            var request = service.Events.Update(@event, calendarID, @event.Id);
            return await request.ExecuteAsync();
        }

        public async Task<string> Delete(Event @event, string calendarID)
        {
            var request = service.Events.Delete(calendarID, @event.Id);
            return await request.ExecuteAsync();
        }
    }
}
