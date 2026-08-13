namespace NurseryManagementSystem.Application.Features.Plans.DTOs
{
    public record PlanDto(
        Guid Id,
        string Name,
        int DurationHours,
        bool IsWeekend,
        decimal MonthlyFee,
        decimal DailyOvertimeFee);
}
