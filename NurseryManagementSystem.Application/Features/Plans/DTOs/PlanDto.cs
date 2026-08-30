namespace NurseryManagementSystem.Application.Features.Plans.DTOs
{
    public record PlanDto(
        Guid Id,
        string Name,
        int DurationHours,
        bool IsWeekend,
        decimal MonthlyFee,
        decimal DailyOvertimeFee,
        string Category,
        string BillingCycle,
        int DaysPerCycle,
        bool IsFullDay,
        string? BadgeText,
        bool IsFeatured,
        bool IsActive,
        string Currency,
        int DisplayOrder)
    {
        public decimal Price => MonthlyFee;
    }
}
