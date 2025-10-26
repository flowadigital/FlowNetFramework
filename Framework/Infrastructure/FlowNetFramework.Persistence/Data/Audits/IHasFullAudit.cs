using System.ComponentModel.DataAnnotations.Schema;

namespace FlowNetFramework.Persistence.Data.Audits
{
    public class IHasFullAudit
    {
        [Column(TypeName = "varchar(100)")]
        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        [Column(TypeName = "varchar(100)")]
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
