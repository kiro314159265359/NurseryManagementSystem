using MediatR;
using Microsoft.EntityFrameworkCore;
using NurseryManagementSystem.Application.Common.Interfaces;
using NurseryManagementSystem.Application.Common.Models;
using NurseryManagementSystem.Application.Features.Attendance.DTOs;
using NurseryManagementSystem.Domain.Entities.Attendance;

namespace NurseryManagementSystem.Application.Features.Attendance.Queries
{
    public record GetStaffAttendanceQuery(
        Guid? UserId = null,
        DateOnly? From = null,
        DateOnly? To = null,
        int PageNumber = 1,
        int PageSize = 20) : IRequest<PaginatedList<StaffAttendanceDto>>;

    public class GetStaffAttendanceQueryHandler
        : IRequestHandler<GetStaffAttendanceQuery, PaginatedList<StaffAttendanceDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetStaffAttendanceQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PaginatedList<StaffAttendanceDto>> Handle(
            GetStaffAttendanceQuery request,
            CancellationToken cancellationToken)
        {
            var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
            var pageSize = request.PageSize is < 1 or > 200 ? 20 : request.PageSize;

            var query = _unitOfWork.Repository<StaffAttendance>().Query().AsNoTracking();

            if (request.UserId is not null)
            {
                query = query.Where(a => a.UserId == request.UserId.Value);
            }

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

            var items = records
                .Select(a => new StaffAttendanceDto(
                    a.Id,
                    a.UserId,
                    a.ClockIn,
                    a.ClockOut,
                    a.AttendanceDate,
                    a.ScanType.ToString()))
                .ToList();

            return new PaginatedList<StaffAttendanceDto>(items, count, pageNumber, pageSize);
        }
    }
}
