using System.ComponentModel.DataAnnotations.Schema;

namespace FlowNetFramework.Data.Audits
{
    public abstract class IHasFullAudit
    {
        [Column(TypeName = "varchar(30)")]
        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        [Column(TypeName = "varchar(30)")]
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }
}
