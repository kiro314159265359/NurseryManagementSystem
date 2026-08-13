using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NurseryManagementSystem.Application.Common.Exceptions;
using NurseryManagementSystem.Application.Common.Interfaces;
using NurseryManagementSystem.Application.Features.Attendance.DTOs;
using NurseryManagementSystem.Domain.Entities.Attendance;
using NurseryManagementSystem.Domain.Enums;

namespace NurseryManagementSystem.Application.Features.Attendance.Commands
{
    public record StaffCheckInCommand(string Code, ScanType ScanType) : IRequest<StaffAttendanceDto>;

    public class StaffCheckInCommandValidator : AbstractValidator<StaffCheckInCommand>
    {
        public StaffCheckInCommandValidator()
        {
            RuleFor(x => x.Code).NotEmpty();
            RuleFor(x => x.ScanType).IsInEnum();
        }
    }

    public class StaffCheckInCommandHandler : IRequestHandler<StaffCheckInCommand, StaffAttendanceDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IIdentityService _identityService;
        private readonly IDateTimeProvider _dateTime;

        public StaffCheckInCommandHandler(
            IUnitOfWork unitOfWork,
            IIdentityService identityService,
            IDateTimeProvider dateTime)
        {
            _unitOfWork = unitOfWork;
            _identityService = identityService;
            _dateTime = dateTime;
        }

        public async Task<StaffAttendanceDto> Handle(StaffCheckInCommand request, CancellationToken cancellationToken)
        {
            var user = await _identityService.Users
                .FirstOrDefaultAsync(u => u.QrCode == request.Code, cancellationToken);

            if (user is null)
            {
                throw new NotFoundException($"No staff member found for code '{request.Code}'.");
            }

            if (!user.IsActive)
            {
                throw new ConflictException("This staff member is not active.");
            }

            var attendanceRepo = _unitOfWork.Repository<StaffAttendance>();

            var hasOpen = await attendanceRepo.AnyAsync(
                a => a.UserId == user.Id && a.ClockOut == null,
                cancellationToken);

            if (hasOpen)
            {
                throw new ConflictException("This staff member is already checked in.");
            }

            var now = _dateTime.UtcNow;

            var attendance = new StaffAttendance
            {
                UserId = user.Id,
                ClockIn = now,
                AttendanceDate = DateOnly.FromDateTime(now),
                ScanType = request.ScanType
            };

            await attendanceRepo.AddAsync(attendance, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new StaffAttendanceDto(
                attendance.Id,
                attendance.UserId,
                attendance.ClockIn,
                attendance.ClockOut,
                attendance.AttendanceDate,
                attendance.ScanType.ToString());
        }
    }
}
