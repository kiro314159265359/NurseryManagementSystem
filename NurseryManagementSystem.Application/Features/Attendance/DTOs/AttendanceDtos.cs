namespace NurseryManagementSystem.Application.Features.Attendance.DTOs
{
    public record ChildAttendanceDto(
        Guid Id,
        Guid ChildId,
        DateTime ClockIn,
        DateTime? ClockOut,
        DateOnly AttendanceDate,
        decimal HoursStayed,
        decimal OvertimeHours,
        decimal OvertimeFee,
        string ScanType);

    public record StaffAttendanceDto(
        Guid Id,
        Guid UserId,
        DateTime ClockIn,
        DateTime? ClockOut,
        DateOnly AttendanceDate,
        string ScanType);
}
