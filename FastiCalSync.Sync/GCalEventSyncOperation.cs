using System;
using System.Threading.Tasks;

namespace FastiCalSync.Sync
{
    public interface IGCalEventSyncOperation
    {
        string GCalID { get; }
        double Priority { get; }
        Task Execute(GCalService service);
    }

    public class ImportGCalEventOperation : IGCalEventSyncOperation
    {
        private iCalEvent iCalEvent;

        public ImportGCalEventOperation(
            iCalEvent @event, string gCalID)
        {
            this.iCalEvent = @event;
            this.GCalID = gCalID;
        }

        public string GCalID { get; private set; }
        public double Priority => iCalEvent.GetSyncPriority();

        public async Task Execute(GCalService service)
        {
            GCalEvent googleEvent = new GCalEvent();
            iCalEvent.To(googleEvent);
            await service.Import(googleEvent.InnerEvent, GCalID);
        }

        public override string ToString()
            => $"import {iCalEvent.Uid}";
    }

    public class UpdateGCalEventOperation : IGCalEventSyncOperation
    {
        private iCalEvent iCalEvent;
        private GCalEvent gCalEvent;

        public UpdateGCalEventOperation(
            iCalEvent iCalEvent, GCalEvent gCalEvent, string gCalID)
        {
            this.iCalEvent = iCalEvent;
            this.gCalEvent = gCalEvent;
            this.GCalID = gCalID;
        }

        public string GCalID { get; private set; }
        public double Priority => Math.Min(iCalEvent.GetSyncPriority(), gCalEvent.GetSyncPriority());

        public async Task Execute(GCalService service)
        {
            iCalEvent.To(gCalEvent);
            await service.Update(gCalEvent.InnerEvent, GCalID);
        }

        public override string ToString()
            => $"update {gCalEvent.Uid}";
    }

    public class DeleteGCalEventOperation : IGCalEventSyncOperation
    {
        private GCalEvent gCalEvent;

        public DeleteGCalEventOperation(
            GCalEvent gCalEvent, string gCalID)
        {
            this.gCalEvent = gCalEvent;
            this.GCalID = gCalID;
        }

        public string GCalID { get; private set; }
        public double Priority => gCalEvent.GetSyncPriority();

        public async Task Execute(GCalService service)
        {
            await service.Delete(gCalEvent.InnerEvent, GCalID);
        }

        public override string ToString()
            => $"delete {gCalEvent.Uid}";
    }
}
