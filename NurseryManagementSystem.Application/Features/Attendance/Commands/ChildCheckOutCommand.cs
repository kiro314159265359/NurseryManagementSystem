using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NurseryManagementSystem.Application.Common.Exceptions;
using NurseryManagementSystem.Application.Common.Interfaces;
using NurseryManagementSystem.Application.Features.Attendance.DTOs;
using NurseryManagementSystem.Domain.Entities.Attendance;
using NurseryManagementSystem.Domain.Entities.Children;
using NurseryManagementSystem.Domain.Entities.Plans;
using NurseryManagementSystem.Domain.Entities.Nursery;

namespace NurseryManagementSystem.Application.Features.Attendance.Commands
{
    public record ChildCheckOutCommand(string Code) : IRequest<ChildAttendanceDto>;

    public class ChildCheckOutCommandValidator : AbstractValidator<ChildCheckOutCommand>
    {
        public ChildCheckOutCommandValidator()
        {
            RuleFor(x => x.Code).NotEmpty();
        }
    }

    public class ChildCheckOutCommandHandler : IRequestHandler<ChildCheckOutCommand, ChildAttendanceDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateTimeProvider _dateTime;

        public ChildCheckOutCommandHandler(IUnitOfWork unitOfWork, IDateTimeProvider dateTime)
        {
            _unitOfWork = unitOfWork;
            _dateTime = dateTime;
        }

        public async Task<ChildAttendanceDto> Handle(ChildCheckOutCommand request, CancellationToken cancellationToken)
        {
            var child = await _unitOfWork.Repository<Child>()
                .FirstOrDefaultAsync(c => c.QrCode == request.Code, cancellationToken);

            if (child is null)
            {
                throw new NotFoundException($"No child found for code '{request.Code}'.");
            }

            var attendanceRepo = _unitOfWork.Repository<ChildAttendance>();

            var attendance = await attendanceRepo.Query()
                .Where(a => a.ChildId == child.Id && a.ClockOut == null)
                .OrderByDescending(a => a.ClockIn)
                .FirstOrDefaultAsync(cancellationToken);

            if (attendance is null)
            {
                throw new ConflictException("This child has no open attendance to check out.");
            }

            var now = _dateTime.UtcNow;
            var today = DateOnly.FromDateTime(now);

            attendance.ClockOut = now;

            var hours = (decimal)(now - attendance.ClockIn).TotalHours;
            attendance.HoursStayed = Math.Round(hours < 0 ? 0 : hours, 2);

            var activeAssignment = await _unitOfWork.Repository<ChildPlanAssignment>().Query()
                .Include(a => a.Plan)
                .Where(a => a.ChildId == child.Id
                            && a.StartDate <= today
                            && (a.EndDate == null || a.EndDate >= today))
                .OrderByDescending(a => a.StartDate)
                .FirstOrDefaultAsync(cancellationToken);

            if (activeAssignment?.Plan is not null)
            {
                var overtime = attendance.HoursStayed - activeAssignment.Plan.DurationHours;
                if (overtime > 0)
                {
                    attendance.OvertimeHours = Math.Round(overtime, 2);
                    var settings = await _unitOfWork.Repository<NurserySettings>().Query()
                        .AsNoTracking().FirstOrDefaultAsync(cancellationToken);
                    var rate = activeAssignment.Plan.DailyOvertimeFee > 0
                        ? activeAssignment.Plan.DailyOvertimeFee
                        : settings?.OvertimeHourlyRate ?? 0m;
                    attendance.OvertimeFee = Math.Round(attendance.OvertimeHours * rate, 2);
                }
            }

            attendanceRepo.Update(attendance);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new ChildAttendanceDto(
                attendance.Id,
                attendance.ChildId,
                attendance.ClockIn,
                attendance.ClockOut,
                attendance.AttendanceDate,
                attendance.HoursStayed,
                attendance.OvertimeHours,
                attendance.OvertimeFee,
                attendance.ScanType.ToString());
        }
    }
}
