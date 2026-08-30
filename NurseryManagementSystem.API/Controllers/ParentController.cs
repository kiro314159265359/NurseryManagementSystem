using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NurseryManagementSystem.Application.Common.Interfaces;
using NurseryManagementSystem.Application.Features.Attendance.Queries;
using NurseryManagementSystem.Application.Features.Billing.Queries;
using NurseryManagementSystem.Application.Features.Children.Commands;
using NurseryManagementSystem.Application.Features.Children.Models;
using NurseryManagementSystem.Domain.Entities.Attendance;
using NurseryManagementSystem.Domain.Entities.Billing;
using NurseryManagementSystem.Domain.Entities.Children;
using NurseryManagementSystem.Domain.Entities.Plans;
using NurseryManagementSystem.Domain.Entities.Schedule;

namespace NurseryManagementSystem.API.Controllers;

[Authorize(Roles = "Parent")]
public class ParentController : ApiControllerBase
{
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public ParentController(ICurrentUserService currentUser, IUnitOfWork unitOfWork)
    {
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    [HttpGet("children")]
    public async Task<IActionResult> GetChildren(CancellationToken cancellationToken)
    {
        var parentId = GetParentId();
        var children = await _unitOfWork.Repository<ParentChild>().Query()
            .AsNoTracking()
            .Where(x => x.ParentUserId == parentId)
            .Select(x => new
            {
                x.Child.Id,
                x.Child.FullName,
                x.Child.DateOfBirth,
                x.Child.EnrollmentDate,
                x.Child.QrCode,
                x.Child.IsActive,
                x.Relationship,
                Status = x.Child.IsActive ? "Approved" : "PendingApproval"
            })
            .OrderBy(x => x.FullName)
            .ToListAsync(cancellationToken);

        return Ok(children);
    }

    [HttpPost("children")]
    public async Task<IActionResult> EnrollChild(ParentEnrollmentRequest request, CancellationToken cancellationToken)
    {
        var parentId = GetParentId();
        var childId = await Mediator.Send(new CreateChildCommand(
            request.FullName,
            request.DateOfBirth,
            DateOnly.FromDateTime(DateTime.UtcNow),
            request.Nationality,
            request.Religion,
            request.HomeAddress,
            request.Allergies,
            request.Mother,
            request.Father,
            request.Agreement,
            request.EmergencyContacts), cancellationToken);

        var child = await _unitOfWork.Repository<Child>().GetByIdAsync(childId, cancellationToken)
            ?? throw new InvalidOperationException("The enrolled child could not be loaded.");
        child.IsActive = false;
        _unitOfWork.Repository<Child>().Update(child);

        await _unitOfWork.Repository<ParentChild>().AddAsync(new ParentChild
        {
            ParentUserId = parentId,
            ChildId = childId,
            Relationship = string.IsNullOrWhiteSpace(request.Relationship)
                ? "Parent"
                : request.Relationship.Trim()
        }, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetDashboard), new { childId }, new
        {
            id = childId,
            status = "PendingApproval"
        });
    }

    [HttpGet("children/{childId:guid}/dashboard")]
    public async Task<IActionResult> GetDashboard(Guid childId, CancellationToken cancellationToken)
    {
        await EnsureOwnsChild(childId, cancellationToken);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var child = await _unitOfWork.Repository<Child>().Query()
            .AsNoTracking()
            .Where(x => x.Id == childId)
            .Select(x => new { x.Id, x.FullName, x.DateOfBirth, x.QrCode, x.IsActive })
            .SingleAsync(cancellationToken);

        var attendance = await _unitOfWork.Repository<ChildAttendance>().Query()
            .AsNoTracking()
            .Where(x => x.ChildId == childId && x.AttendanceDate == today)
            .OrderByDescending(x => x.ClockIn)
            .Select(x => new
            {
                x.ClockIn,
                x.ClockOut,
                x.HoursStayed,
                x.OvertimeHours,
                IsCheckedIn = x.ClockOut == null
            })
            .FirstOrDefaultAsync(cancellationToken);

        var activePlan = await _unitOfWork.Repository<ChildPlanAssignment>().Query()
            .AsNoTracking()
            .Where(x => x.ChildId == childId && x.StartDate <= today &&
                        (x.EndDate == null || x.EndDate >= today))
            .OrderByDescending(x => x.StartDate)
            .Select(x => new
            {
                x.Id,
                x.PlanId,
                x.Plan.Name,
                x.Plan.DurationHours,
                x.Plan.IsWeekend,
                x.Plan.MonthlyFee,
                x.Plan.DailyOvertimeFee,
                x.StartDate,
                x.EndDate
            })
            .FirstOrDefaultAsync(cancellationToken);

        var outstandingBalance = await _unitOfWork.Repository<MonthlyInvoice>().Query()
            .AsNoTracking()
            .Where(x => x.ChildId == childId && x.Status == Domain.Enums.InvoiceStatus.Pending)
            .SumAsync(x => (decimal?)x.GrandTotal, cancellationToken) ?? 0m;

        var schedule = await _unitOfWork.Repository<DailyScheduleSlot>().Query()
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.SortOrder)
            .Select(x => new
            {
                x.Id,
                x.StartTime,
                x.EndTime,
                x.ActivityName,
                x.Description
            })
            .ToListAsync(cancellationToken);

        return Ok(new { child, attendance, activePlan, outstandingBalance, schedule });
    }

    [HttpGet("children/{childId:guid}/attendance")]
    public async Task<IActionResult> GetAttendance(
        Guid childId, DateOnly? from, DateOnly? to, int pageNumber = 1, int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        await EnsureOwnsChild(childId, cancellationToken);
        return Ok(await Mediator.Send(
            new GetChildAttendanceQuery(childId, from, to, pageNumber, pageSize), cancellationToken));
    }

    [HttpGet("children/{childId:guid}/invoices")]
    public async Task<IActionResult> GetInvoices(
        Guid childId, int? year, int? month, int pageNumber = 1, int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        await EnsureOwnsChild(childId, cancellationToken);
        return Ok(await Mediator.Send(
            new GetInvoicesQuery(childId, null, year, month, pageNumber, pageSize), cancellationToken));
    }

    [HttpPost("children/{childId:guid}/plans/{planId:guid}")]
    public async Task<IActionResult> SelectPlan(
        Guid childId, Guid planId, CancellationToken cancellationToken)
    {
        var parentId = GetParentId();
        await EnsureOwnsChild(childId, cancellationToken, requireApproved: true);
        var plan = await _unitOfWork.Repository<SubscriptionPlan>().GetByIdAsync(planId, cancellationToken);
        if (plan is null)
        {
            return NotFound(new { message = "Plan not found." });
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var current = await _unitOfWork.Repository<ChildPlanAssignment>().FirstOrDefaultAsync(
            x => x.ChildId == childId && x.EndDate == null, cancellationToken);
        if (current is not null)
        {
            current.EndDate = today.AddDays(-1);
            _unitOfWork.Repository<ChildPlanAssignment>().Update(current);
        }

        var assignment = new ChildPlanAssignment
        {
            ChildId = childId,
            PlanId = planId,
            StartDate = today,
            AssignedById = parentId
        };
        await _unitOfWork.Repository<ChildPlanAssignment>().AddAsync(assignment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Ok(new { id = assignment.Id });
    }

    private Guid GetParentId()
        => _currentUser.UserId ?? throw new UnauthorizedAccessException();

    private async Task EnsureOwnsChild(
        Guid childId, CancellationToken cancellationToken, bool requireApproved = false)
    {
        var parentId = GetParentId();
        var owned = await _unitOfWork.Repository<ParentChild>().Query()
            .AsNoTracking()
            .AnyAsync(x => x.ParentUserId == parentId && x.ChildId == childId &&
                           (!requireApproved || x.Child.IsActive), cancellationToken);
        if (!owned)
        {
            throw new UnauthorizedAccessException("You do not have access to this child.");
        }
    }
}

public record ParentEnrollmentRequest(
    string FullName,
    DateOnly DateOfBirth,
    string Nationality,
    string Religion,
    string HomeAddress,
    string? Allergies,
    string Relationship,
    ParentInput Mother,
    ParentInput Father,
    AgreementInput Agreement,
    IReadOnlyList<EmergencyContactInput> EmergencyContacts);
