using Microsoft.WindowsAzure.Storage.Table;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FastiCalSync.Data
{
    public static class CloudTableExtensions
    {
        public static async Task<IList<T>> ExecuteQueryAsync<T>(this CloudTable table, TableQuery<T> query)
            where T : ITableEntity, new()
        {
            TableContinuationToken token = null;
            var results = new List<T>();

            do
            {
                var segment = await table.ExecuteQuerySegmentedAsync<T>(query, token);
                results.AddRange(segment);
                token = segment.ContinuationToken;
            }
            while (token != null);

            return results;
        }
    }
}
