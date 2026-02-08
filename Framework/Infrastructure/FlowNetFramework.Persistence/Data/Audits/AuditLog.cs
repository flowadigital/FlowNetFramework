namespace FlowNetFramework.Persistence.Data.Audits
{
    public class AuditLog : BaseEntity
    {
        public string EntityName { get; set; } = default!;
        public string EntityId { get; set; } = default!;

        public string Action { get; set; }

        public string? OldValueJson { get; set; }
        public string? NewValueJson { get; set; }

        public string UserId { get; set; } = default!;
    }
}
