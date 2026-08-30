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
        string ScanType,
        int? AllowedHours = null,
        string? CheckedInByName = null,
        string? CheckedOutByName = null,
        string? Source = null);

    public record TodayAttendanceDto(
        Guid ChildId,
        string ChildFullName,
        string? PhotoUrl,
        string? PlanName,
        int? AllowedHours,
        bool IsCheckedIn,
        DateTime? CheckedInAt,
        DateTime? CheckedOutAt,
        decimal HoursOnSite,
        decimal OvertimeHours);

    public record AttendanceTodaySummary(int CheckedIn, int CheckedOut, int TotalEnrolled);

    public record AttendanceTodayResponse(
        IReadOnlyList<TodayAttendanceDto> Items,
        int TotalCount,
        int PageNumber,
        int PageSize,
        int TotalPages,
        AttendanceTodaySummary Summary);

    public record StaffAttendanceDto(
        Guid Id,
        Guid UserId,
        DateTime ClockIn,
        DateTime? ClockOut,
        DateOnly AttendanceDate,
        string ScanType);
}
