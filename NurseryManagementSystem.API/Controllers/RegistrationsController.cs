using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NurseryManagementSystem.Application.Features.Registrations.Commands;
using NurseryManagementSystem.Application.Features.Registrations.DTOs;
using NurseryManagementSystem.Application.Features.Registrations.Queries;

namespace NurseryManagementSystem.API.Controllers
{
    public class RegistrationsController : ApiControllerBase
    {
        [AllowAnonymous]
        [HttpPost("self")]
        public async Task<ActionResult<RegistrationCreatedDto>> SelfRegister(
            SelfRegisterFamilyCommand command)
        {
            var result = await Mediator.Send(command);
            return StatusCode(StatusCodes.Status201Created, result);
        }

        [Authorize(Roles = "SuperAdmin,SubAdmin")]
        [HttpPost("admin")]
        public async Task<ActionResult<RegistrationCreatedDto>> AdminRegister(
            AdminCreateFamilyRegistrationCommand command)
        {
            var result = await Mediator.Send(command);
            return StatusCode(StatusCodes.Status201Created, result);
        }

        [Authorize(Roles = "Parent")]
        [HttpPost("children")]
        public async Task<ActionResult<RegistrationCreatedDto>> AddChild(
            SubmitChildRegistrationCommand command)
        {
            var result = await Mediator.Send(command);
            return StatusCode(StatusCodes.Status201Created, result);
        }

        [Authorize(Roles = "Parent")]
        [HttpGet("mine")]
        public async Task<ActionResult<IReadOnlyList<RegistrationDto>>> Mine()
            => Ok(await Mediator.Send(new GetMyRegistrationsQuery()));

        [Authorize(Roles = "SuperAdmin,SubAdmin")]
        [HttpGet("pending")]
        public async Task<ActionResult<IReadOnlyList<RegistrationDto>>> Pending()
            => Ok(await Mediator.Send(new GetPendingRegistrationsQuery()));

        [Authorize(Roles = "SuperAdmin,SubAdmin")]
        [HttpPut("{childId:guid}/approve")]
        public async Task<IActionResult> Approve(Guid childId)
        {
            await Mediator.Send(new ReviewRegistrationCommand(childId, true));
            return NoContent();
        }

        [Authorize(Roles = "SuperAdmin,SubAdmin")]
        [HttpPut("{childId:guid}/reject")]
        public async Task<IActionResult> Reject(Guid childId, RejectRegistrationRequest request)
        {
            await Mediator.Send(new ReviewRegistrationCommand(childId, false, request.Reason));
            return NoContent();
        }
    }

    public record RejectRegistrationRequest(string Reason);
}
