using System.ComponentModel.DataAnnotations.Schema;

namespace FlowNetFramework.Persistence.Data.Audits
{
    public class IHasFullAudit
    {
        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public Guid? TenantId { get; set; }
    }
}
