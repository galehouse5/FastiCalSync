using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace FastiCalSync.Data
{
    public class Token : TableEntity
    {
        public bool HasGoogleAccessToken { get; set; }
        public string GoogleAccessToken { get; set; }
        public string GoogleRefreshToken { get; set; }
        public DateTime? GoogleTokenExpirationUtc { get; set; }
        public string GoogleTokenType { get; set; }
    }
}
