namespace FlowNetFramework.Data.Audits
{
    public interface ISoftDeletable
    {
        bool IsActive { get; set; }
    }
}
