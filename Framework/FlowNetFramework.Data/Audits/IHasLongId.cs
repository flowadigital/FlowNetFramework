using System.ComponentModel.DataAnnotations;

namespace FlowNetFramework.Data.Audits
{
    public interface IHasLongId
    {
        [Key]
        public long Id { get; set; }
    }
}
