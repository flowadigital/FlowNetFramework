using System.ComponentModel.DataAnnotations;

namespace FlowNetFramework.Persistence.Data.Audits
{
    public interface IHasLongId
    {
        [Key]
        public long Id { get; set; }
    }
}
