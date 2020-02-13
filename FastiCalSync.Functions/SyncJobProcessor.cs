using FastiCalSync.Data;
using FastiCalSync.Sync;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FastiCalSync.Functions
{
    public static class SyncJobProcessor
    {
        public static string AzureWebJobsStorage
            => Environment.GetEnvironmentVariable("AzureWebJobsStorage");

        public static int MaxSyncOperationsPerJob
            => int.Parse(Environment.GetEnvironmentVariable("MaxSyncOperationsPerJob"));

        public static string GoogleClientID
            => Environment.GetEnvironmentVariable("GoogleClientID");

        public static string GoogleClientSecret
            => Environment.GetEnvironmentVariable("GoogleClientSecret");

        [FunctionName("SyncJobProcessor")]
        public static async Task Run(
            [QueueTrigger("sync-jobs", Connection = "AzureWebJobsStorage")]string json,
            TraceWriter log,
            Microsoft.Azure.WebJobs.ExecutionContext context)
        {
            SyncJob job = SyncJob.Create(json);
            var calendarRepository = new CalendarRepository(AzureWebJobsStorage);
            var tokenRepository = new TokenRepository(AzureWebJobsStorage);

            log.Info($"Reading calendar {job.CalendarID} for {job.UserName}...");
            Calendar calendar = await calendarRepository.Read(job.UserName, job.CalendarID);

            try
            {
                log.Info($"Reading Google token for {job.UserName}...");
                Token token = await tokenRepository.Read(job.UserName);

                log.Info($"Refreshing Google token if needed...");
                UserCredential credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    new ClientSecrets
                    {
                        ClientId = GoogleClientID,
                        ClientSecret = GoogleClientSecret
                    },
                    new[] { CalendarService.Scope.Calendar },
                    "user",
                    CancellationToken.None,
                    new TokenDataStore(token, tokenRepository));

                GCalService gCalService = new GCalService(credential);

                if (calendar.SyncState == SyncState.Deleting)
                {
                    if (calendar.GoogleCalendarID != null)
                    {
                        log.Info("Deleting Google calendar...");
                        await gCalService.DeleteCalendar(calendar.GoogleCalendarID);
                    }

                    log.Info("Deleting calendar record...");
                    await calendarRepository.Delete(calendar);
                }
                else if (calendar.SyncState == SyncState.Syncing)
                {
                    iCalService iCalService = new iCalService();
                    GCalEventSyncer eventSyncer = new GCalEventSyncer(gCalService);
                    CalendarSyncer syncer = new CalendarSyncer(
                        gCalService, eventSyncer, iCalService, calendarRepository,
                        logger: m => log.Info(m));

                    log.Info("Syncing calendar...");
                    await syncer.Execute(calendar, eventLimit: MaxSyncOperationsPerJob);

                    log.Info("Saving calendar record...");
                    calendar.RecordJobSuccess();
                    await calendarRepository.Update(calendar);
                }
                else throw new NotSupportedException($"Unable to process calendar in '{calendar.SyncState}' state.");
            }
            catch (Exception ex)
            {
                log.Info("Logging error...");
                calendar.RecordJobError(ex);
                await calendarRepository.Update(calendar);
            }
        }
    }
}
