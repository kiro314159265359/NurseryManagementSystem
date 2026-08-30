using NurseryManagementSystem.Domain.Common;

namespace NurseryManagementSystem.Domain.Entities.Audit;

public class AuditLogEntry : BaseEntity
{
    public DateTime At { get; set; }
    public Guid? ActorUserId { get; set; }
    public string ActorName { get; set; } = "system";
    public string Action { get; set; } = string.Empty;
    public string SubjectType { get; set; } = string.Empty;
    public Guid SubjectId { get; set; }
    public string? SubjectName { get; set; }
    public decimal? Amount { get; set; }
    public string? Details { get; set; }
}
