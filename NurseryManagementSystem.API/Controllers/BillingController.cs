using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NurseryManagementSystem.Application.Common.Models;
using NurseryManagementSystem.Application.Features.Billing.Commands;
using NurseryManagementSystem.Application.Features.Billing.DTOs;
using NurseryManagementSystem.Application.Features.Billing.Queries;
using NurseryManagementSystem.Domain.Enums;
using NurseryManagementSystem.Application.Common.Interfaces;
using NurseryManagementSystem.Domain.Entities.Billing;
using NurseryManagementSystem.Domain.Entities.Nursery;
using NurseryManagementSystem.Application.Common.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace NurseryManagementSystem.API.Controllers
{
    [Authorize(Roles = "SuperAdmin,SubAdmin")]
    public class BillingController : ApiControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public BillingController(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;
        [HttpPost("generate")]
        public async Task<IActionResult> Generate(GenerateMonthlyInvoicesCommand command)
        {
            var count = await Mediator.Send(command);
            return Ok(new { generated = count });
        }

        [HttpGet("invoices")]
        public async Task<ActionResult<PaginatedList<InvoiceDto>>> GetInvoices(
            Guid? childId = null,
            InvoiceStatus? status = null,
            int? year = null,
            int? month = null,
            int pageNumber = 1,
            int pageSize = 20,
            string? search = null)
            => Ok(await Mediator.Send(new GetInvoicesQuery(childId, status, year, month, pageNumber, pageSize, search)));

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary(int month, int year, CancellationToken cancellationToken)
        {
            var query = _unitOfWork.Repository<MonthlyInvoice>().Query().AsNoTracking()
                .Where(i => i.BillingMonth == month && i.BillingYear == year);
            var rows = await query.ToListAsync(cancellationToken);
            return Ok(new
            {
                totalInvoiced = rows.Where(i => i.Status != InvoiceStatus.Cancelled).Sum(i => i.GrandTotal),
                totalCollected = rows.Where(i => i.Status == InvoiceStatus.Paid).Sum(i => i.GrandTotal),
                totalOutstanding = rows.Where(i => i.Status is InvoiceStatus.Pending or InvoiceStatus.Overdue).Sum(i => i.GrandTotal),
                invoiceCount = rows.Count,
                paidCount = rows.Count(i => i.Status == InvoiceStatus.Paid),
                unpaidCount = rows.Count(i => i.Status == InvoiceStatus.Pending),
                overdueCount = rows.Count(i => i.Status == InvoiceStatus.Overdue),
                currency = "AED"
            });
        }

        [HttpGet("revenue")]
        public async Task<IActionResult> GetRevenue(DateOnly from, DateOnly to, string granularity = "Month", CancellationToken cancellationToken = default)
        {
            var rows = await _unitOfWork.Repository<MonthlyInvoice>().Query().AsNoTracking()
                .Where(i => i.Status == InvoiceStatus.Paid
                    && i.PaidAt >= from.ToDateTime(TimeOnly.MinValue)
                    && i.PaidAt < to.AddDays(1).ToDateTime(TimeOnly.MinValue))
                .ToListAsync(cancellationToken);
            var points = rows.GroupBy(i => granularity.Equals("Day", StringComparison.OrdinalIgnoreCase)
                    ? i.PaidAt!.Value.ToString("yyyy-MM-dd")
                    : i.PaidAt!.Value.ToString("yyyy-MM"))
                .OrderBy(g => g.Key)
                .Select(g => new { period = g.Key, revenue = g.Sum(i => i.GrandTotal), overtimeRevenue = g.Sum(i => i.TotalOvertimeFee) });
            return Ok(new { points, currency = "AED" });
        }

        [HttpGet("invoices/{id:guid}")]
        public async Task<ActionResult<InvoiceDto>> GetInvoice(Guid id)
            => Ok(await Mediator.Send(new GetInvoiceByIdQuery(id)));

        [HttpPut("invoices/{id:guid}/pay")]
        public async Task<IActionResult> MarkPaid(Guid id)
        {
            await Mediator.Send(new MarkInvoicePaidCommand(id));
            return NoContent();
        }

        [HttpPut("invoices/{id:guid}/cancel")]
        public async Task<IActionResult> Cancel(Guid id)
        {
            await Mediator.Send(new CancelInvoiceCommand(id));
            return NoContent();
        }

        [HttpPut("invoices/{id:guid}/adjust")]
        public async Task<IActionResult> Adjust(Guid id, AdjustInvoiceRequest request, CancellationToken cancellationToken)
        {
            var invoice = await _unitOfWork.Repository<MonthlyInvoice>().GetByIdAsync(id, cancellationToken)
                ?? throw new NotFoundException("MonthlyInvoice", id);
            if (invoice.Status is InvoiceStatus.Paid or InvoiceStatus.Cancelled)
                throw new ConflictException("Only unpaid invoices can be adjusted.");
            var settings = await _unitOfWork.Repository<NurserySettings>().Query().AsNoTracking().FirstOrDefaultAsync(cancellationToken);
            var overtimeRate = invoice.OvertimeRate > 0 ? invoice.OvertimeRate : settings?.OvertimeHourlyRate ?? 0m;
            if (request.OvertimeHoursOverride is decimal hours)
                invoice.TotalOvertimeFee = Math.Max(0m, hours) * overtimeRate;
            if (request.PenaltyOverride is decimal penalty)
                invoice.PenaltyAmount = Math.Max(0m, penalty);
            invoice.AdjustmentAmount = request.AdjustmentAmount ?? 0m;
            invoice.AdjustmentReason = request.Reason.Trim();
            invoice.GrandTotal = invoice.PlanFee + invoice.TotalOvertimeFee + invoice.PenaltyAmount + invoice.AdjustmentAmount;
            _unitOfWork.Repository<MonthlyInvoice>().Update(invoice);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Ok(await Mediator.Send(new GetInvoiceByIdQuery(id), cancellationToken));
        }
    }

    public record AdjustInvoiceRequest(
        decimal? OvertimeHoursOverride,
        decimal? PenaltyOverride,
        decimal? AdjustmentAmount,
        string Reason);
}
