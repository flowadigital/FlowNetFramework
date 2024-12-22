using System.ComponentModel.DataAnnotations.Schema;

namespace FlowNetFramework.Data.Audits
{
    public class IHasFullAudit
    {
        [Column(TypeName = "varchar(30)")]
        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        [Column(TypeName = "varchar(30)")]
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
