namespace NurseryManagementSystem.Application.Features.Schedule.DTOs
{
    public record ScheduleSlotDto(
        Guid Id,
        TimeOnly StartTime,
        TimeOnly EndTime,
        string ActivityName,
        string? Description,
        int SortOrder,
        bool IsActive);
}
