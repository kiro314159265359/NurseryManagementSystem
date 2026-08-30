using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NurseryManagementSystem.Application.Common.Models;
using NurseryManagementSystem.Application.Features.Children.Commands;
using NurseryManagementSystem.Application.Features.Children.DTOs;
using NurseryManagementSystem.Application.Features.Children.Queries;

namespace NurseryManagementSystem.API.Controllers
{
    [Authorize(Roles = "SuperAdmin,SubAdmin")]
    public class ChildrenController : ApiControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<PaginatedList<ChildDto>>> Get(
            int pageNumber = 1,
            int pageSize = 20,
            string? search = null,
            bool activeOnly = false)
            => Ok(await Mediator.Send(new GetChildrenQuery(pageNumber, pageSize, search, activeOnly)));

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ChildDetailsDto>> GetById(Guid id)
            => Ok(await Mediator.Send(new GetChildByIdQuery(id)));

        [HttpPost]
        public async Task<IActionResult> Create(CreateChildCommand command)
        {
            var id = await Mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id }, new { id });
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, UpdateChildCommand command)
        {
            await Mediator.Send(command with { Id = id });
            return NoContent();
        }

        [HttpPut("{id:guid}/active")]
        public async Task<IActionResult> SetActive(Guid id, SetChildActiveCommand command)
        {
            await Mediator.Send(command with { Id = id });
            return NoContent();
        }

        [HttpPost("{childId:guid}/emergency-contacts")]
        public async Task<IActionResult> AddEmergencyContact(Guid childId, AddEmergencyContactCommand command)
        {
            var id = await Mediator.Send(command with { ChildId = childId });
            return Ok(new { id });
        }

        [HttpDelete("{childId:guid}/emergency-contacts/{contactId:guid}")]
        public async Task<IActionResult> RemoveEmergencyContact(Guid childId, Guid contactId)
        {
            await Mediator.Send(new RemoveEmergencyContactCommand(childId, contactId));
            return NoContent();
        }
    }
}
