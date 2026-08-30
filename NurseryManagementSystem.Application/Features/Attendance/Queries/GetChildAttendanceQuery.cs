using MediatR;
using Microsoft.EntityFrameworkCore;
using NurseryManagementSystem.Application.Common.Interfaces;
using NurseryManagementSystem.Application.Common.Models;
using NurseryManagementSystem.Application.Features.Attendance.DTOs;
using NurseryManagementSystem.Domain.Entities.Attendance;
using NurseryManagementSystem.Domain.Entities.Plans;

namespace NurseryManagementSystem.Application.Features.Attendance.Queries
{
    public record GetChildAttendanceQuery(
        Guid ChildId,
        DateOnly? From = null,
        DateOnly? To = null,
        int PageNumber = 1,
        int PageSize = 20) : IRequest<PaginatedList<ChildAttendanceDto>>;

    public class GetChildAttendanceQueryHandler
        : IRequestHandler<GetChildAttendanceQuery, PaginatedList<ChildAttendanceDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetChildAttendanceQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PaginatedList<ChildAttendanceDto>> Handle(
            GetChildAttendanceQuery request,
            CancellationToken cancellationToken)
        {
            var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
            var pageSize = request.PageSize is < 1 or > 200 ? 20 : request.PageSize;

            var query = _unitOfWork.Repository<ChildAttendance>().Query()
                .AsNoTracking()
                .Where(a => a.ChildId == request.ChildId);

            if (request.From is not null)
            {
                query = query.Where(a => a.AttendanceDate >= request.From.Value);
            }

            if (request.To is not null)
            {
                query = query.Where(a => a.AttendanceDate <= request.To.Value);
            }

            var count = await query.CountAsync(cancellationToken);

            var records = await query
                .OrderByDescending(a => a.ClockIn)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var assignments = await _unitOfWork.Repository<ChildPlanAssignment>().Query()
                .AsNoTracking()
                .Include(a => a.Plan)
                .Where(a => a.ChildId == request.ChildId)
                .OrderByDescending(a => a.StartDate)
                .ToListAsync(cancellationToken);

            var items = records
                .Select(a =>
                {
                    var assignment = assignments.FirstOrDefault(x =>
                        x.StartDate <= a.AttendanceDate && (x.EndDate == null || x.EndDate >= a.AttendanceDate));
                    var allowedHours = assignment is null ? (int?)null
                        : assignment.DurationHoursSnapshot > 0
                            ? assignment.DurationHoursSnapshot
                            : assignment.Plan.DurationHours;
                    return new ChildAttendanceDto(
                    a.Id,
                    a.ChildId,
                    a.ClockIn,
                    a.ClockOut,
                    a.AttendanceDate,
                    a.HoursStayed,
                    a.OvertimeHours,
                    a.OvertimeFee,
                    a.ScanType.ToString(),
                    allowedHours,
                    null,
                    null,
                    a.ScanType == Domain.Enums.ScanType.Manual ? "Manual" : "Scan");
                })
                .ToList();

            return new PaginatedList<ChildAttendanceDto>(items, count, pageNumber, pageSize);
        }
    }
}
