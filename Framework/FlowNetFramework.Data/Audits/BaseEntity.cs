namespace FlowNetFramework.Data.Audits
{
    public class BaseEntity : IHasFullAudit, IHasLongId, ISoftDeletable
    {
        public BaseEntity()
        {
            Guid = Guid.NewGuid();
        }

        public long Id { get; set; }
        public Guid Guid { get; set; }
        public bool IsActive { get; set; }
    }
}
