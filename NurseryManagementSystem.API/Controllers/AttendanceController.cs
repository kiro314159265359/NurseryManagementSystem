using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NurseryManagementSystem.Application.Common.Models;
using NurseryManagementSystem.Application.Features.Attendance.Commands;
using NurseryManagementSystem.Application.Features.Attendance.DTOs;
using NurseryManagementSystem.Application.Features.Attendance.Queries;

namespace NurseryManagementSystem.API.Controllers
{
    [Authorize(Roles = "SuperAdmin,SubAdmin")]
    public class AttendanceController : ApiControllerBase
    {
        [HttpPost("children/check-in")]
        public async Task<ActionResult<ChildAttendanceDto>> ChildCheckIn(ChildCheckInCommand command)
            => Ok(await Mediator.Send(command));

        [HttpPost("children/check-out")]
        public async Task<ActionResult<ChildAttendanceDto>> ChildCheckOut(ChildCheckOutCommand command)
            => Ok(await Mediator.Send(command));

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
