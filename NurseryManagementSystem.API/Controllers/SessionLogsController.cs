using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NurseryManagementSystem.Application.Common.Models;
using NurseryManagementSystem.Application.Features.SessionLogs.DTOs;
using NurseryManagementSystem.Application.Features.SessionLogs.Queries;

namespace NurseryManagementSystem.API.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class SessionLogsController : ApiControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<PaginatedList<SessionLogDto>>> Get(
            Guid? userId = null,
            int pageNumber = 1,
            int pageSize = 20)
            => Ok(await Mediator.Send(new GetSessionLogsQuery(userId, pageNumber, pageSize)));
    }
}
