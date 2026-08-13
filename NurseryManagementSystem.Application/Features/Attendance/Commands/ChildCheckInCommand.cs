using FluentValidation;
using MediatR;
using NurseryManagementSystem.Application.Common.Exceptions;
using NurseryManagementSystem.Application.Common.Interfaces;
using NurseryManagementSystem.Application.Features.Attendance.DTOs;
using NurseryManagementSystem.Domain.Entities.Attendance;
using NurseryManagementSystem.Domain.Entities.Children;
using NurseryManagementSystem.Domain.Enums;

namespace NurseryManagementSystem.Application.Features.Attendance.Commands
{
    public record ChildCheckInCommand(string Code, ScanType ScanType) : IRequest<ChildAttendanceDto>;

    public class ChildCheckInCommandValidator : AbstractValidator<ChildCheckInCommand>
    {
        public ChildCheckInCommandValidator()
        {
            RuleFor(x => x.Code).NotEmpty();
            RuleFor(x => x.ScanType).IsInEnum();
        }
    }

    public class ChildCheckInCommandHandler : IRequestHandler<ChildCheckInCommand, ChildAttendanceDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateTimeProvider _dateTime;

        public ChildCheckInCommandHandler(IUnitOfWork unitOfWork, IDateTimeProvider dateTime)
        {
            _unitOfWork = unitOfWork;
            _dateTime = dateTime;
        }

        public async Task<ChildAttendanceDto> Handle(ChildCheckInCommand request, CancellationToken cancellationToken)
        {
            var child = await _unitOfWork.Repository<Child>()
                .FirstOrDefaultAsync(c => c.QrCode == request.Code, cancellationToken);

            if (child is null)
            {
                throw new NotFoundException($"No child found for code '{request.Code}'.");
            }

            if (!child.IsActive)
            {
                throw new ConflictException("This child is not active.");
            }

            var attendanceRepo = _unitOfWork.Repository<ChildAttendance>();

            var hasOpen = await attendanceRepo.AnyAsync(
                a => a.ChildId == child.Id && a.ClockOut == null,
                cancellationToken);

            if (hasOpen)
            {
                throw new ConflictException("This child is already checked in.");
            }

            var now = _dateTime.UtcNow;

            var attendance = new ChildAttendance
            {
                ChildId = child.Id,
                ClockIn = now,
                AttendanceDate = DateOnly.FromDateTime(now),
                ScanType = request.ScanType
            };

            await attendanceRepo.AddAsync(attendance, cancellationToken);
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
