using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FastiCalSync.Data
{
    public class CalendarRepository
    {
        private readonly CloudTableClient client;

        public CalendarRepository(string connectionString)
        {
            this.client = CloudStorageAccount.Parse(connectionString).CreateCloudTableClient();
        }

        public async Task Create(Calendar value)
        {
            CloudTable table = client.GetTableReference("Calendars");
            await table.ExecuteAsync(TableOperation.Insert(value));
        }

        public async Task<IReadOnlyCollection<Calendar>> Read(string userName)
        {
            CloudTable table = client.GetTableReference("Calendars");
            var query = new TableQuery<Calendar>()
                .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, userName));
            return (await table.ExecuteQueryAsync(query)).ToArray();
        }

        public async Task<IReadOnlyCollection<Calendar>> ReadForProcessing()
        {
            CloudTable table = client.GetTableReference("Calendars");
            var query = new TableQuery<Calendar>()
                .Where(TableQuery.CombineFilters(
                    TableQuery.GenerateFilterConditionForInt("InternalSyncState", QueryComparisons.Equal, (int)SyncState.Syncing),
                    TableOperators.Or,
                    TableQuery.GenerateFilterConditionForInt("InternalSyncState", QueryComparisons.Equal, (int)SyncState.Deleting)));
            return (await table.ExecuteQueryAsync(query)).ToArray();
        }

        public async Task<Calendar> Read(string userName, string calendarID)
        {
            CloudTable table = client.GetTableReference("Calendars");
            TableOperation operation = TableOperation.Retrieve<Calendar>(userName, calendarID);
            return (await table.ExecuteAsync(operation)).Result as Calendar;
        }

        public async Task Update(Calendar value)
        {
            CloudTable table = client.GetTableReference("Calendars");
            await table.ExecuteAsync(TableOperation.Replace(value));
        }

        public async Task Delete(Calendar value)
        {
            CloudTable table = client.GetTableReference("Calendars");
            await table.ExecuteAsync(TableOperation.Delete(value));
        }
    }
}
