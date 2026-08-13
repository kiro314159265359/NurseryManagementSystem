using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NurseryManagementSystem.Application.Features.PlanAssignments.Commands;
using NurseryManagementSystem.Application.Features.PlanAssignments.DTOs;
using NurseryManagementSystem.Application.Features.PlanAssignments.Queries;

namespace NurseryManagementSystem.API.Controllers
{
    [Authorize]
    public class PlanAssignmentsController : ApiControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Assign(AssignPlanCommand command)
        {
            var id = await Mediator.Send(command);
            return Ok(new { id });
        }

        [HttpPut("{id:guid}/end")]
        public async Task<IActionResult> End(Guid id, EndPlanAssignmentCommand command)
        {
            await Mediator.Send(command with { AssignmentId = id });
            return NoContent();
        }

        [HttpGet("child/{childId:guid}")]
        public async Task<ActionResult<IReadOnlyList<PlanAssignmentDto>>> GetForChild(Guid childId)
            => Ok(await Mediator.Send(new GetChildAssignmentsQuery(childId)));
    }
}
