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
            try
            {
                await CreateGCalIfNotExists(calendar);

                logger("Reading iCal data...");
                var iCal = await iCalService.Read(new Uri(calendar.iCalendarUrl));
                calendar.LastSyncSourceEventCount = iCal.Events.Count;

                logger("Determining events to sync...");
                var events = await gCalEventSyncer.GetEventSyncOperations(calendar.GoogleCalendarID, iCal);
                calendar.LastSyncRemainingImportCount = events.OfType<ImportGCalEventOperation>().Count();
                calendar.LastSyncRemainingUpdateCount = events.OfType<UpdateGCalEventOperation>().Count();
                calendar.LastSyncRemainingDeleteCount = events.OfType<DeleteGCalEventOperation>().Count();
                calendar.LastSyncImportCount = 0;
                calendar.LastSyncUpdateCount = 0;
                calendar.LastSyncDeleteCount = 0;

                if (events.Any())
                {
                    var eventsToSync = events.Take(eventLimit).ToArray();
                    logger($"Syncing {eventsToSync.Count()} events...");

                    foreach (var @event in eventsToSync)
                    {
                        await @event.Execute(gCalService);

                        calendar.LastSyncRemainingImportCount -= @event is ImportGCalEventOperation ? 1 : 0;
                        calendar.LastSyncRemainingUpdateCount -= @event is UpdateGCalEventOperation ? 1 : 0;
                        calendar.LastSyncRemainingDeleteCount -= @event is DeleteGCalEventOperation ? 1 : 0;
                        calendar.LastSyncImportCount += @event is ImportGCalEventOperation ? 1 : 0;
                        calendar.LastSyncUpdateCount += @event is UpdateGCalEventOperation ? 1 : 0;
                        calendar.LastSyncDeleteCount += @event is DeleteGCalEventOperation ? 1 : 0;
                        calendar.TotalSyncImportCount += @event is ImportGCalEventOperation ? 1 : 0;
                        calendar.TotalSyncUpdateCount += @event is UpdateGCalEventOperation ? 1 : 0;
                        calendar.TotalSyncDeleteCount += @event is DeleteGCalEventOperation ? 1 : 0;
                    }
                }
                else
                {
                    logger("No events to sync, exiting...");
                }

                calendar.LastSyncTimestampUtc = DateTime.UtcNow;
                calendar.LastSyncErrorRetryCount = null;
            }
            catch (Exception ex)
            {
                logger("Logging error...");
                calendar.LastSyncErrorTimestampUtc = DateTime.UtcNow;
                calendar.LastSyncErrorMessage = ex.Message;
                calendar.LastSyncErrorStackTrace = ex.StackTrace;
                calendar.LastSyncErrorRetryCount = (calendar.LastSyncErrorRetryCount + 1) ?? 0;

                if (calendar.LastSyncErrorRetryCount >= 2)
                {
                    calendar.SyncState = SyncState.PausedByError;
                }
            }
            finally
            {
                logger("Saving calendar record...");
                await calendarRepository.Update(calendar);
            }
        }
    }
}
