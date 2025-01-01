using System.ComponentModel.DataAnnotations;

namespace FlowNetFramework.Persistence.Data.Audits
{
    public interface IHasGenericId<T>
    {
        [Key]
        public T Id { get; set; }
    }
}
