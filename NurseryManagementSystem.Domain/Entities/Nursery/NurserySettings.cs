using NurseryManagementSystem.Domain.Common;

namespace NurseryManagementSystem.Domain.Entities.Nursery;

public class NurserySettings : AuditableEntity
{
    public string NurseryName { get; set; } = "Wildwood Nursery";
    public int Capacity { get; set; } = 50;
    public string Currency { get; set; } = "AED";
    public decimal OvertimeHourlyRate { get; set; } = 25m;
    public int LatePickupGraceMinutes { get; set; } = 15;
    public decimal LatePickupFinePerDay { get; set; } = 50m;
    public TimeOnly OpeningTime { get; set; } = new(7, 0);
    public TimeOnly ClosingTime { get; set; } = new(17, 0);
    public string TimeZone { get; set; } = "Asia/Dubai";
}
