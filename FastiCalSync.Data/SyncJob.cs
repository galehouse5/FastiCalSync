using Newtonsoft.Json;

namespace FastiCalSync.Data
{
    public class SyncJob
    {
        public SyncJob(Calendar calendar)
        {
            UserName = calendar.UserName;
            CalendarID = calendar.ID;
        }

        /// <summary>
        /// For internal use only.
        /// </summary>
        public SyncJob()
        { }

        public string UserName { get; set; }
        public string CalendarID { get; set; }

        public string ToJson()
            => JsonConvert.SerializeObject(this);

        public static SyncJob Create(string json)
            => JsonConvert.DeserializeObject<SyncJob>(json);
    }
}
