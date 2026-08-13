using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NurseryManagementSystem.Application.Common.Models;
using NurseryManagementSystem.Application.Features.Users.Commands;
using NurseryManagementSystem.Application.Features.Users.DTOs;
using NurseryManagementSystem.Application.Features.Users.Queries;

namespace NurseryManagementSystem.API.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class UsersController : ApiControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<PaginatedList<UserDto>>> Get(
            int pageNumber = 1,
            int pageSize = 20,
            string? search = null)
            => Ok(await Mediator.Send(new GetUsersQuery(pageNumber, pageSize, search)));

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<UserDto>> GetById(Guid id)
            => Ok(await Mediator.Send(new GetUserByIdQuery(id)));

        [HttpPost]
        public async Task<IActionResult> Create(CreateUserCommand command)
        {
            var id = await Mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id }, new { id });
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, UpdateUserCommand command)
        {
            await Mediator.Send(command with { Id = id });
            return NoContent();
        }

        [HttpPut("{id:guid}/role")]
        public async Task<IActionResult> AssignRole(Guid id, AssignRoleCommand command)
        {
            await Mediator.Send(command with { Id = id });
            return NoContent();
        }

        [HttpPut("{id:guid}/active")]
        public async Task<IActionResult> SetActive(Guid id, SetUserActiveCommand command)
        {
            await Mediator.Send(command with { Id = id });
            return NoContent();
        }

        [HttpPut("{id:guid}/password")]
        public async Task<IActionResult> ChangePassword(Guid id, ChangePasswordCommand command)
        {
            await Mediator.Send(command with { Id = id });
            return NoContent();
        }
    }
}
