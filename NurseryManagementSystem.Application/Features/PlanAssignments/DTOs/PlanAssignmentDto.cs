namespace NurseryManagementSystem.Application.Features.PlanAssignments.DTOs
{
    public record PlanAssignmentDto(
        Guid Id,
        Guid ChildId,
        Guid PlanId,
        string PlanName,
        DateOnly StartDate,
        DateOnly? EndDate,
        Guid AssignedById);
}
