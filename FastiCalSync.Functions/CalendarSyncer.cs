using FastiCalSync.Data;
using FastiCalSync.Sync;
using System;
using System.Linq;
using System.Threading.Tasks;
using GCal = Google.Apis.Calendar.v3.Data;

namespace FastiCalSync.Functions
{
    public class CalendarSyncer
    {
        private readonly GCalService gCalService;
        private readonly GCalEventSyncer gCalEventSyncer;
        private readonly iCalService iCalService;
        private readonly CalendarRepository calendarRepository;
        private readonly Action<string> logger;

        public CalendarSyncer(
            GCalService gCalService,
            GCalEventSyncer gCalEventSyncer,
            iCalService iCalService,
            CalendarRepository calendarRepository,
            Action<string> logger)
        {
            this.gCalService = gCalService;
            this.gCalEventSyncer = gCalEventSyncer;
            this.iCalService = iCalService;
            this.calendarRepository = calendarRepository;
            this.logger = logger;
        }

        protected async Task CreateGCalIfNotExists(Calendar calendar)
        {
            if (calendar.GoogleCalendarID != null) return;

            var gCal = new GCal.Calendar
            {
                Summary = calendar.iCalendarUrl
            };

            logger("Creating Google calendar...");
            gCal = await gCalService.Create(gCal);

            calendar.GoogleCalendarID = gCal.Id;
            logger($"Saving Google calendar ID...");
            await calendarRepository.Update(calendar);
        }

        public async Task Execute(Calendar calendar, int eventLimit = 100)
        {
            await CreateGCalIfNotExists(calendar);

            logger("Reading iCal data...");
            var iCal = await iCalService.Read(new Uri(calendar.iCalendarUrl));
            calendar.iCalendarEventCount = iCal.Events.Count;

            logger("Determining events to sync...");
            var events = await gCalEventSyncer.GetEventSyncOperations(calendar.GoogleCalendarID, iCal);

            using (var countTracker = calendar.BeginSyncCountTracking(
                remainingCreateCount: events.OfType<ImportGCalEventOperation>().Count(),
                remainingUpdateCount: events.OfType<UpdateGCalEventOperation>().Count(),
                remainingDeleteCount: events.OfType<DeleteGCalEventOperation>().Count()))
            {
                if (events.Any())
                {
                    var eventsToSync = events.Take(eventLimit).ToArray();
                    logger($"Syncing {eventsToSync.Count()} events...");

                    foreach (var @event in eventsToSync)
                    {
                        await @event.Execute(gCalService);

                        countTracker.CreateCount += @event is ImportGCalEventOperation ? 1 : 0;
                        countTracker.UpdateCount += @event is UpdateGCalEventOperation ? 1 : 0;
                        countTracker.DeleteCount += @event is DeleteGCalEventOperation ? 1 : 0;
                    }
                }
                else
                {
                    logger("No events to sync.");
                }
            }
        }
    }
}
