using System.ComponentModel.DataAnnotations;

namespace FlowNetFramework.Persistence.Data.Audits
{
    public class BaseEntity : IHasFullAudit, IHasGuidId, ISoftDeletable
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public bool IsActive { get; set; }
    }
}
