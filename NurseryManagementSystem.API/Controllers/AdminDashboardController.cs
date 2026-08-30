using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NurseryManagementSystem.Application.Common.Interfaces;
using NurseryManagementSystem.Domain.Entities.Attendance;
using NurseryManagementSystem.Domain.Entities.Billing;
using NurseryManagementSystem.Domain.Entities.Children;
using NurseryManagementSystem.Domain.Enums;

namespace NurseryManagementSystem.API.Controllers;

[Authorize(Roles = "SuperAdmin,SubAdmin")]
public class AdminDashboardController : ApiControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public AdminDashboardController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var now = DateTime.UtcNow;

        var activeChildren = await _unitOfWork.Repository<Child>().CountAsync(
            x => x.IsActive, cancellationToken);
        var pendingEnrollments = await _unitOfWork.Repository<Child>().CountAsync(
            x => !x.IsActive, cancellationToken);
        var childrenPresent = await _unitOfWork.Repository<ChildAttendance>().Query()
            .AsNoTracking()
            .Where(x => x.AttendanceDate == today && x.ClockOut == null)
            .Select(x => x.ChildId)
            .Distinct()
            .CountAsync(cancellationToken);
        var staffPresent = await _unitOfWork.Repository<StaffAttendance>().Query()
            .AsNoTracking()
            .Where(x => x.AttendanceDate == today && x.ClockOut == null)
            .Select(x => x.UserId)
            .Distinct()
            .CountAsync(cancellationToken);
        var pendingInvoices = await _unitOfWork.Repository<MonthlyInvoice>().CountAsync(
            x => x.Status == InvoiceStatus.Pending, cancellationToken);
        var outstandingBalance = await _unitOfWork.Repository<MonthlyInvoice>().Query()
            .AsNoTracking()
            .Where(x => x.Status == InvoiceStatus.Pending)
            .SumAsync(x => (decimal?)x.GrandTotal, cancellationToken) ?? 0m;
        var monthlyRevenue = await _unitOfWork.Repository<MonthlyInvoice>().Query()
            .AsNoTracking()
            .Where(x => x.Status == InvoiceStatus.Paid && x.PaidAt != null &&
                        x.PaidAt.Value.Year == now.Year && x.PaidAt.Value.Month == now.Month)
            .SumAsync(x => (decimal?)x.GrandTotal, cancellationToken) ?? 0m;

        return Ok(new
        {
            activeChildren,
            pendingEnrollments,
            childrenPresent,
            staffPresent,
            pendingInvoices,
            outstandingBalance,
            monthlyRevenue,
            generatedAt = now
        });
    }

    [HttpGet("pending-enrollments")]
    public async Task<IActionResult> GetPendingEnrollments(CancellationToken cancellationToken)
    {
        var pending = await _unitOfWork.Repository<Child>().Query()
            .AsNoTracking()
            .Where(x => !x.IsActive)
            .OrderBy(x => x.CreatedAt)
            .Select(x => new
            {
                x.Id,
                x.FullName,
                x.DateOfBirth,
                x.EnrollmentDate,
                x.Nationality,
                x.Allergies,
                Parent = x.ParentLinks.Select(link => new
                {
                    link.ParentUser.FullName,
                    link.ParentUser.Email,
                    link.ParentUser.PhoneNumber,
                    link.Relationship
                }).FirstOrDefault(),
                x.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return Ok(pending);
    }

    [HttpPut("pending-enrollments/{childId:guid}/approve")]
    public async Task<IActionResult> Approve(Guid childId, CancellationToken cancellationToken)
    {
        var child = await _unitOfWork.Repository<Child>().GetByIdAsync(childId, cancellationToken);
        if (child is null)
        {
            return NotFound();
        }

        child.IsActive = true;
        _unitOfWork.Repository<Child>().Update(child);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}
