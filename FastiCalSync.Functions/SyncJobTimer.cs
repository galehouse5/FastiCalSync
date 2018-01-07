using FastiCalSync.Data;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FastiCalSync.Functions
{
    public static class SyncJobTimer
    {
        public static string AzureWebJobsStorage
            => Environment.GetEnvironmentVariable("AzureWebJobsStorage");

        [FunctionName("SyncJobTimer")]
        public static async Task Run(
            [TimerTrigger("0 * * * * *")]TimerInfo timer,
            TraceWriter log)
        {
            var repository = new CalendarRepository(AzureWebJobsStorage);
            var queue = new SyncQueue(AzureWebJobsStorage);

            log.Info("Reading unpaused calendars...");
            var calendars = await repository.ReadWhereUnpaused();

            log.Info($"Queueing sync jobs for {calendars.Count():n0} calendars...");
            Task.WaitAll(calendars
                .Select(c => new SyncJob(c))
                .Select(queue.Enqueue)
                .ToArray());
        }
    }
}