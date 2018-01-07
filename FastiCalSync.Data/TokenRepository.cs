using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Threading.Tasks;

namespace FastiCalSync.Data
{
    public class TokenRepository
    {
        private readonly CloudTableClient client;

        public TokenRepository(string connectionString)
        {
            this.client = CloudStorageAccount.Parse(connectionString).CreateCloudTableClient();
        }

        public async Task<Token> Read(string userName)
        {
            string key = $"U_{Uri.EscapeDataString(userName).Replace("%", "_").ToUpper()}";
            CloudTable table = client.GetTableReference("AspNetUsers");
            TableOperation operation = TableOperation.Retrieve<Token>(key, key);
            return (await table.ExecuteAsync(operation)).Result as Token;
        }

        public async Task Update(Token value)
        {
            CloudTable table = client.GetTableReference("AspNetUsers");
            await table.ExecuteAsync(TableOperation.Merge(value));
        }
    }
}
