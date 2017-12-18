using Ical.Net;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace FastiCalSync.Shared
{
    public class iCalService
    {
        public async Task<Calendar> GetCalendar(Uri url)
        {
            using (HttpClient client = new HttpClient())
            using (Stream data = await client.GetStreamAsync(url))
            {
                return Calendar.Load(data);
            }
        }
    }
}
