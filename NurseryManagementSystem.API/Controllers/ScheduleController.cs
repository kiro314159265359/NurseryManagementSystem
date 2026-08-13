using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NurseryManagementSystem.Application.Features.Schedule.Commands;
using NurseryManagementSystem.Application.Features.Schedule.DTOs;
using NurseryManagementSystem.Application.Features.Schedule.Queries;

namespace NurseryManagementSystem.API.Controllers
{
    [Authorize]
    public class ScheduleController : ApiControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<ScheduleSlotDto>>> Get(bool activeOnly = false)
            => Ok(await Mediator.Send(new GetScheduleQuery(activeOnly)));

        [HttpPost]
        public async Task<IActionResult> Create(CreateScheduleSlotCommand command)
        {
            var id = await Mediator.Send(command);
            return Ok(new { id });
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, UpdateScheduleSlotCommand command)
        {
            await Mediator.Send(command with { Id = id });
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await Mediator.Send(new DeleteScheduleSlotCommand(id));
            return NoContent();
        }
    }
}
