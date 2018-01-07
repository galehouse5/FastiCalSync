using Ical.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace FastiCalSync.Sync.Tests
{
    [TestClass]
    public class iCalServiceTests
    {
        private readonly iCalService service = new iCalService();

        [TestMethod]
        public async Task LoadsCalendarFromUrl()
        {
            Calendar calendar = await service.Read(new Uri(Registry.AppSettings["iCalUrl"]));

            Assert.IsNotNull(calendar);
            Assert.AreNotEqual(0, calendar.Events.Count);
        }
    }
}
