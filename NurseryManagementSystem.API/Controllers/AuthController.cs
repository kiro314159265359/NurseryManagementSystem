using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NurseryManagementSystem.Application.Features.Auth.Commands;
using NurseryManagementSystem.Application.Features.Auth.DTOs;
using NurseryManagementSystem.Application.Features.SessionLogs.Commands;

namespace NurseryManagementSystem.API.Controllers
{
    public class AuthController : ApiControllerBase
    {
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login(LoginCommand command)
        {
            var result = await Mediator.Send(command);

            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            await Mediator.Send(new CreateSessionLogCommand(result.UserId, ipAddress));

            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<ActionResult<AuthResponse>> Refresh(RefreshTokenCommand command)
            => Ok(await Mediator.Send(command));

        [Authorize]
        [HttpPost("revoke")]
        public async Task<IActionResult> Revoke(RevokeTokenCommand command)
        {
            await Mediator.Send(command);
            return NoContent();
        }
    }
}
