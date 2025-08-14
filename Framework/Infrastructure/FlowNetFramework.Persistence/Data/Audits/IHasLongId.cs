using System.ComponentModel.DataAnnotations;

namespace FlowNetFramework.Persistence.Data.Audits
{
    public interface IHasGuidId
    {
        [Key]
        public Guid Id { get; set; }
    }
}
