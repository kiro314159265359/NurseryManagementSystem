using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NurseryManagementSystem.Application.Common.Interfaces;
using NurseryManagementSystem.Application.Common.Models;
using NurseryManagementSystem.Domain.Entities.Audit;

namespace NurseryManagementSystem.API.Controllers;

[Route("api/audit-log")]
[Authorize(Roles = "SuperAdmin")]
public class AuditLogController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    public AuditLogController(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    [HttpGet]
    public async Task<IActionResult> Get(
        DateTime? from = null, DateTime? to = null, Guid? userId = null, string? action = null,
        int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var query = _unitOfWork.Repository<AuditLogEntry>().Query().AsNoTracking();
        if (from is not null) query = query.Where(x => x.At >= from);
        if (to is not null) query = query.Where(x => x.At <= to);
        if (userId is not null) query = query.Where(x => x.ActorUserId == userId);
        if (!string.IsNullOrWhiteSpace(action)) query = query.Where(x => x.Action == action);
        return Ok(await PaginatedList<AuditLogEntry>.CreateAsync(
            query.OrderByDescending(x => x.At), Math.Max(1, pageNumber), pageSize is < 1 or > 200 ? 20 : pageSize, cancellationToken));
    }
}
