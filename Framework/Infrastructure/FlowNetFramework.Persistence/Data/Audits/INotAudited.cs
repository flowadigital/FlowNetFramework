namespace FlowNetFramework.Persistence.Data.Audits
{
    /// <summary>
    /// Bu işaretleyiciden türeyen entity'ler Created/Updated stamp alır ama AuditLog satırı yazılmaz.
    /// Aggregate'in kullanıcıya görünmeyen kabuk kaydı (ör. Inquiry) için kullanılır.
    /// </summary>
    public interface INotAudited
    {
    }
}
