using FastiCalSync.Shared;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Util.Store;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FastiCalSync.Tests
{
    [TestClass]
    public class EventSyncerTests
    {
        private iCalService iCalService;
        private GCalService gCalService;
        private EventSyncer eventSyncer;

        [TestInitialize]
        public async Task Initialize()
        {
            iCalService = new iCalService();

            using (Stream stream = new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
            {
                string credentialPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                    ".credentials/calendar-dotnet-quickstart.json");
                UserCredential credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    new[] { CalendarService.Scope.Calendar },
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credentialPath, true));
                gCalService = new GCalService(credential);
            }

            eventSyncer = new EventSyncer(iCalService, gCalService);
        }

        [TestMethod]
        public async Task ExecutesEventSyncOperations()
        {
            Uri iCalUrl = new Uri(Registry.AppSettings["iCalUrl"]);
            string gCalID = await eventSyncer.GetOrCreateGCalID(iCalUrl);
            var operations = await eventSyncer.GetEventSyncOperations(gCalID, iCalUrl);

            foreach (IEventSyncOperation operation in operations.Take(10))
            {
                await operation.Execute(gCalService);
            }
        }
    }
} 
