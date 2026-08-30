using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NurseryManagementSystem.Application.Common.Interfaces;
using NurseryManagementSystem.Domain.Entities.Attendance;
using NurseryManagementSystem.Domain.Entities.Billing;
using NurseryManagementSystem.Domain.Entities.Children;
using NurseryManagementSystem.Domain.Entities.Nursery;
using NurseryManagementSystem.Domain.Enums;

namespace NurseryManagementSystem.API.Controllers;

[Authorize(Roles = "SuperAdmin,SubAdmin")]
public class DashboardController : ApiControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    public DashboardController(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    [HttpGet("summary")]
    public async Task<ActionResult<DashboardSummaryDto>> Summary(CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(now);
        var settings = await _unitOfWork.Repository<NurserySettings>().Query().AsNoTracking().FirstOrDefaultAsync(cancellationToken);
        var enrolled = await _unitOfWork.Repository<Child>().CountAsync(
            x => x.IsActive && x.ApprovalStatus == ApprovalStatus.Approved, cancellationToken);
        var todayRows = await _unitOfWork.Repository<ChildAttendance>().Query().AsNoTracking()
            .Where(x => x.AttendanceDate == today).ToListAsync(cancellationToken);
        var invoices = _unitOfWork.Repository<MonthlyInvoice>().Query().AsNoTracking();
        var revenueToday = await invoices.Where(x => x.Status == InvoiceStatus.Paid && x.PaidAt != null
                && x.PaidAt >= now.Date && x.PaidAt < now.Date.AddDays(1))
            .SumAsync(x => (decimal?)x.GrandTotal, cancellationToken) ?? 0m;
        var outstanding = await invoices.Where(x => x.Status == InvoiceStatus.Pending || x.Status == InvoiceStatus.Overdue)
            .SumAsync(x => (decimal?)x.GrandTotal, cancellationToken) ?? 0m;
        var unpaid = await invoices.CountAsync(x => x.Status == InvoiceStatus.Pending || x.Status == InvoiceStatus.Overdue, cancellationToken);
        var pending = await _unitOfWork.Repository<Child>().CountAsync(x => x.ApprovalStatus == ApprovalStatus.Pending, cancellationToken);

        return Ok(new DashboardSummaryDto(
            today,
            todayRows.Count(x => x.ClockOut == null),
            settings?.Capacity ?? 50,
            enrolled,
            todayRows.Select(x => x.ChildId).Distinct().Count(),
            todayRows.Sum(x => x.ClockOut == null
                ? Math.Max(0m, (decimal)(now - x.ClockIn).TotalHours) : x.HoursStayed),
            todayRows.Sum(x => x.OvertimeHours),
            revenueToday,
            outstanding,
            unpaid,
            pending,
            settings?.Currency ?? "AED"));
    }

    [HttpGet("alerts")]
    public async Task<ActionResult<DashboardAlertsDto>> Alerts(CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var live = await _unitOfWork.Repository<ChildAttendance>().Query().AsNoTracking()
            .Where(x => x.AttendanceDate == today && x.ClockOut == null)
            .Select(x => new DashboardAlertDto(
                "OvertimeLive", x.ChildId, x.Child.FullName,
                x.Child.ParentUser != null ? x.Child.ParentUser.FullName : x.Child.Mother.FullName,
                x.Child.ParentUser != null ? x.Child.ParentUser.PhoneNumber : x.Child.Mother.Phone,
                x.OvertimeHours, x.OvertimeFee, x.OvertimeHours > 0))
            .Where(x => x.IsUrgent).ToListAsync(cancellationToken);
        return Ok(new DashboardAlertsDto(live));
    }
}

public record DashboardSummaryDto(
    DateOnly Date, int CheckedInNow, int Capacity, int TotalEnrolled,
    int AttendedToday, decimal ChildHoursToday, decimal OvertimeHoursToday,
    decimal RevenueToday, decimal OutstandingTotal, int UnpaidInvoiceCount,
    int PendingRegistrationsCount, string Currency);

public record DashboardAlertDto(
    string Kind, Guid ChildId, string ChildFullName, string ParentFullName,
    string? ParentPhone, decimal Hours, decimal Amount, bool IsUrgent);

public record DashboardAlertsDto(IReadOnlyList<DashboardAlertDto> Items);
