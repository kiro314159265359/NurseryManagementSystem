using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NurseryManagementSystem.Application.Features.Plans.Commands;
using NurseryManagementSystem.Application.Features.Plans.DTOs;
using NurseryManagementSystem.Application.Features.Plans.Queries;

namespace NurseryManagementSystem.API.Controllers
{
    [Authorize]
    public class PlansController : ApiControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<PlanDto>>> Get()
            => Ok(await Mediator.Send(new GetPlansQuery()));

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<PlanDto>> GetById(Guid id)
            => Ok(await Mediator.Send(new GetPlanByIdQuery(id)));

        [Authorize(Roles = "SuperAdmin")]
        [HttpPost]
        public async Task<IActionResult> Create(CreatePlanCommand command)
        {
            var id = await Mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id }, new { id });
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, UpdatePlanCommand command)
        {
            await Mediator.Send(command with { Id = id });
            return NoContent();
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await Mediator.Send(new DeletePlanCommand(id));
            return NoContent();
        }
    }
}
