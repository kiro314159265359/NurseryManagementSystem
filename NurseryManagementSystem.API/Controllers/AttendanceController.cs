using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NurseryManagementSystem.Application.Common.Models;
using NurseryManagementSystem.Application.Features.Attendance.Commands;
using NurseryManagementSystem.Application.Features.Attendance.DTOs;
using NurseryManagementSystem.Application.Features.Attendance.Queries;
using NurseryManagementSystem.Application.Common.Exceptions;
using NurseryManagementSystem.Application.Common.Interfaces;
using NurseryManagementSystem.Domain.Entities.Children;
using NurseryManagementSystem.Domain.Enums;

namespace NurseryManagementSystem.API.Controllers
{
    [Authorize(Roles = "SuperAdmin,SubAdmin")]
    public class AttendanceController : ApiControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public AttendanceController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        [HttpPost("children/check-in")]
        public async Task<ActionResult<ChildAttendanceDto>> ChildCheckIn(ChildCheckInCommand command)
            => Ok(await Mediator.Send(command));

        [HttpPost("children/check-out")]
        public async Task<ActionResult<ChildAttendanceDto>> ChildCheckOut(ChildCheckOutCommand command)
            => Ok(await Mediator.Send(command));

        [HttpPost("children/{childId:guid}/check-in")]
        public async Task<ActionResult<ChildAttendanceDto>> ManualChildCheckIn(Guid childId)
        {
            var child = await _unitOfWork.Repository<Child>().GetByIdAsync(childId)
                ?? throw new NotFoundException("Child", childId);
            return Ok(await Mediator.Send(new ChildCheckInCommand(child.QrCode, ScanType.Manual)));
        }

        [HttpPost("children/{childId:guid}/check-out")]
        public async Task<ActionResult<ChildAttendanceDto>> ManualChildCheckOut(Guid childId)
        {
            var child = await _unitOfWork.Repository<Child>().GetByIdAsync(childId)
                ?? throw new NotFoundException("Child", childId);
            return Ok(await Mediator.Send(new ChildCheckOutCommand(child.QrCode)));
        }

        [HttpGet("today")]
        public async Task<ActionResult<AttendanceTodayResponse>> GetToday(
            int pageNumber = 1, int pageSize = 20, string? search = null, string status = "All")
            => Ok(await Mediator.Send(new GetTodayAttendanceQuery(pageNumber, pageSize, search, status)));

        [HttpPost("staff/check-in")]
        public async Task<ActionResult<StaffAttendanceDto>> StaffCheckIn(StaffCheckInCommand command)
            => Ok(await Mediator.Send(command));

        [HttpPost("staff/check-out")]
        public async Task<ActionResult<StaffAttendanceDto>> StaffCheckOut(StaffCheckOutCommand command)
            => Ok(await Mediator.Send(command));

        [HttpGet("children/{childId:guid}")]
        public async Task<ActionResult<PaginatedList<ChildAttendanceDto>>> GetChildAttendance(
            Guid childId,
            DateOnly? from = null,
            DateOnly? to = null,
            int pageNumber = 1,
            int pageSize = 20)
            => Ok(await Mediator.Send(new GetChildAttendanceQuery(childId, from, to, pageNumber, pageSize)));

        [HttpGet("staff")]
        public async Task<ActionResult<PaginatedList<StaffAttendanceDto>>> GetStaffAttendance(
            Guid? userId = null,
            DateOnly? from = null,
            DateOnly? to = null,
            int pageNumber = 1,
            int pageSize = 20)
            => Ok(await Mediator.Send(new GetStaffAttendanceQuery(userId, from, to, pageNumber, pageSize)));
    }
}
