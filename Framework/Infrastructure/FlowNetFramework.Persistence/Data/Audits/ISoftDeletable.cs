namespace FlowNetFramework.Persistence.Data.Audits
{
    public interface ISoftDeletable
    {
        bool IsActive { get; set; }
    }
}
