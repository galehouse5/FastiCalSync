using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using System;
using System.Threading.Tasks;

namespace FastiCalSync.Data
{
    public class SyncQueue
    {
        private readonly CloudQueueClient client;

        public SyncQueue(string connectionString)
        {
            this.client = CloudStorageAccount.Parse(connectionString).CreateCloudQueueClient();
        }

        public async Task Enqueue(SyncJob job)
        {
            CloudQueue queue = client.GetQueueReference("sync-jobs");
            CloudQueueMessage message = new CloudQueueMessage(job.ToJson());
            await queue.AddMessageAsync(message,
                timeToLive: TimeSpan.FromSeconds(60),
                initialVisibilityDelay: TimeSpan.Zero,
                options: new QueueRequestOptions { RetryPolicy = new NoRetry() },
                operationContext: null);
        }
    }
}
