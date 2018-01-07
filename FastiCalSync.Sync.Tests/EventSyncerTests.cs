using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Util.Store;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FastiCalSync.Sync.Tests
{
    [TestClass]
    public class EventSyncerTests
    {
        private iCalService iCalService;
        private GCalService gCalService;
        private GCalEventSyncer gCalEventSyncer;

        [TestInitialize]
        public async Task Initialize()
        {
            iCalService = new iCalService();

            string credentialPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                @".credentials\calendar-dotnet-quickstart.json");
            UserCredential credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                new ClientSecrets
                {
                    ClientId = Registry.AppSettings["GoogleClientID"],
                    ClientSecret = Registry.AppSettings["GoogleClientSecret"],
                },
                new[] { CalendarService.Scope.Calendar },
                "user",
                CancellationToken.None,
                new FileDataStore(credentialPath, true));

            gCalService = new GCalService(credential);
            gCalEventSyncer = new GCalEventSyncer(iCalService, gCalService);
        }

        [TestMethod]
        public async Task ExecutesEventSyncOperations()
        {
            Uri iCalUrl = new Uri(Registry.AppSettings["iCalUrl"]);
            var iCal = await iCalService.Read(iCalUrl);
            string gCalID = await gCalEventSyncer.GetOrCreateGCalID(iCalUrl);
            var operations = await gCalEventSyncer.GetEventSyncOperations(gCalID, iCal);

            foreach (var operation in operations.Take(10))
            {
                await operation.Execute(gCalService);
            }
        }
    }
}
