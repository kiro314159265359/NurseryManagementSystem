using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NurseryManagementSystem.Application.Common.Exceptions;
using NurseryManagementSystem.Application.Common.Interfaces;
using NurseryManagementSystem.Application.Features.Attendance.DTOs;
using NurseryManagementSystem.Domain.Entities.Attendance;

namespace NurseryManagementSystem.Application.Features.Attendance.Commands
{
    public record StaffCheckOutCommand(string Code) : IRequest<StaffAttendanceDto>;

    public class StaffCheckOutCommandValidator : AbstractValidator<StaffCheckOutCommand>
    {
        public StaffCheckOutCommandValidator()
        {
            RuleFor(x => x.Code).NotEmpty();
        }
    }

    public class StaffCheckOutCommandHandler : IRequestHandler<StaffCheckOutCommand, StaffAttendanceDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IIdentityService _identityService;
        private readonly IDateTimeProvider _dateTime;

        public StaffCheckOutCommandHandler(
            IUnitOfWork unitOfWork,
            IIdentityService identityService,
            IDateTimeProvider dateTime)
        {
            _unitOfWork = unitOfWork;
            _identityService = identityService;
            _dateTime = dateTime;
        }

        public async Task<StaffAttendanceDto> Handle(StaffCheckOutCommand request, CancellationToken cancellationToken)
        {
            var user = await _identityService.Users
                .FirstOrDefaultAsync(u => u.QrCode == request.Code, cancellationToken);

            if (user is null)
            {
                throw new NotFoundException($"No staff member found for code '{request.Code}'.");
            }

            var attendanceRepo = _unitOfWork.Repository<StaffAttendance>();

            var attendance = await attendanceRepo.Query()
                .Where(a => a.UserId == user.Id && a.ClockOut == null)
                .OrderByDescending(a => a.ClockIn)
                .FirstOrDefaultAsync(cancellationToken);

            if (attendance is null)
            {
                throw new ConflictException("This staff member has no open attendance to check out.");
            }

            attendance.ClockOut = _dateTime.UtcNow;
            attendanceRepo.Update(attendance);
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
