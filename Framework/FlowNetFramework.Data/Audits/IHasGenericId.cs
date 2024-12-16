using System.ComponentModel.DataAnnotations;

namespace FlowNetFramework.Data.Audits
{
    public interface IHasGenericId<T>
    {
        [Key]
        public T Id { get; set; }
    }
}
