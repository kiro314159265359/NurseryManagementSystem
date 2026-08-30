using MediatR;
using Microsoft.EntityFrameworkCore;
using NurseryManagementSystem.Application.Common.Interfaces;
using NurseryManagementSystem.Application.Features.Attendance.DTOs;
using NurseryManagementSystem.Domain.Entities.Children;
using NurseryManagementSystem.Domain.Enums;

namespace NurseryManagementSystem.Application.Features.Attendance.Queries;

public record GetTodayAttendanceQuery(
    int PageNumber = 1,
    int PageSize = 20,
    string? Search = null,
    string Status = "All") : IRequest<AttendanceTodayResponse>;

public class GetTodayAttendanceQueryHandler
    : IRequestHandler<GetTodayAttendanceQuery, AttendanceTodayResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTime;

    public GetTodayAttendanceQueryHandler(IUnitOfWork unitOfWork, IDateTimeProvider dateTime)
    {
        _unitOfWork = unitOfWork;
        _dateTime = dateTime;
    }

    public async Task<AttendanceTodayResponse> Handle(
        GetTodayAttendanceQuery request,
        CancellationToken cancellationToken)
    {
        var now = _dateTime.UtcNow;
        var today = DateOnly.FromDateTime(now);
        var query = _unitOfWork.Repository<Child>().Query()
            .AsNoTracking()
            .Where(c => c.ApprovalStatus == ApprovalStatus.Approved && c.IsActive);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            query = query.Where(c => c.FullName.Contains(term)
                || c.Mother.FullName.Contains(term)
                || c.Father.FullName.Contains(term)
                || c.Mother.Phone.Contains(term)
                || c.Father.Phone.Contains(term));
        }

        var rows = await query
            .Select(c => new
            {
                c.Id,
                c.FullName,
                c.PhotoUrl,
                Attendance = c.Attendances
                    .Where(a => a.AttendanceDate == today)
                    .OrderByDescending(a => a.ClockIn)
                    .FirstOrDefault(),
                Assignment = c.PlanAssignments
                    .Where(a => a.StartDate <= today && (a.EndDate == null || a.EndDate >= today))
                    .OrderByDescending(a => a.StartDate)
                    .Select(a => new { a.Plan.Name, a.Plan.DurationHours })
                    .FirstOrDefault()
            })
            .OrderBy(c => c.FullName)
            .ToListAsync(cancellationToken);

        var allItems = rows.Select(row =>
        {
            var attendance = row.Attendance;
            var isCheckedIn = attendance is not null && attendance.ClockOut == null;
            var hours = attendance is null
                ? 0m
                : attendance.ClockOut is null
                    ? Math.Round((decimal)(now - attendance.ClockIn).TotalHours, 2)
                    : attendance.HoursStayed;
            var overtime = row.Assignment is null
                ? 0m
                : Math.Max(0m, Math.Round(hours - row.Assignment.DurationHours, 2));
            return new TodayAttendanceDto(
                row.Id,
                row.FullName,
                row.PhotoUrl,
                row.Assignment?.Name,
                row.Assignment?.DurationHours,
                isCheckedIn,
                attendance?.ClockIn,
                attendance?.ClockOut,
                hours,
                overtime);
        }).ToList();

        var checkedIn = allItems.Count(x => x.IsCheckedIn);
        var checkedOut = allItems.Count(x => !x.IsCheckedIn);
        IEnumerable<TodayAttendanceDto> filtered = request.Status.Trim().ToLowerInvariant() switch
        {
            "checkedin" => allItems.Where(x => x.IsCheckedIn),
            "checkedout" => allItems.Where(x => !x.IsCheckedIn),
            _ => allItems
        };
        var filteredItems = filtered.ToList();
        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize = request.PageSize is < 1 or > 200 ? 20 : request.PageSize;
        var page = filteredItems.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

        return new AttendanceTodayResponse(
            page,
            filteredItems.Count,
            pageNumber,
            pageSize,
            (int)Math.Ceiling(filteredItems.Count / (double)pageSize),
            new AttendanceTodaySummary(checkedIn, checkedOut, allItems.Count));
    }
}
