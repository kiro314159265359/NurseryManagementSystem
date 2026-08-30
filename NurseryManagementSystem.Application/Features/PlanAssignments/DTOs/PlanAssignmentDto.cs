namespace NurseryManagementSystem.Application.Features.PlanAssignments.DTOs
{
    public record PlanAssignmentDto(
        Guid Id,
        Guid ChildId,
        Guid PlanId,
        string PlanName,
        string PlanCategory,
        decimal Price,
        int DurationHours,
        int DaysPerCycle,
        DateOnly StartDate,
        DateOnly? EndDate,
        bool IsActive,
        Guid AssignedById,
        string AssignedByName,
        DateTime AssignedAt,
        string Currency);
}
