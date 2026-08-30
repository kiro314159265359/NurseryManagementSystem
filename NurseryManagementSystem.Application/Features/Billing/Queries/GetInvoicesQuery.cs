using MediatR;
using Microsoft.EntityFrameworkCore;
using NurseryManagementSystem.Application.Common.Interfaces;
using NurseryManagementSystem.Application.Common.Models;
using NurseryManagementSystem.Application.Features.Billing.DTOs;
using NurseryManagementSystem.Domain.Entities.Billing;
using NurseryManagementSystem.Domain.Enums;

namespace NurseryManagementSystem.Application.Features.Billing.Queries
{
    public record GetInvoicesQuery(
        Guid? ChildId = null,
        InvoiceStatus? Status = null,
        int? Year = null,
        int? Month = null,
        int PageNumber = 1,
        int PageSize = 20,
        string? Search = null) : IRequest<PaginatedList<InvoiceDto>>;

    public class GetInvoicesQueryHandler : IRequestHandler<GetInvoicesQuery, PaginatedList<InvoiceDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetInvoicesQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PaginatedList<InvoiceDto>> Handle(GetInvoicesQuery request, CancellationToken cancellationToken)
        {
            var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
            var pageSize = request.PageSize is < 1 or > 200 ? 20 : request.PageSize;

            var query = _unitOfWork.Repository<MonthlyInvoice>().Query().AsNoTracking();

            if (request.ChildId is not null)
            {
                query = query.Where(i => i.ChildId == request.ChildId.Value);
            }

            if (request.Status is not null)
            {
                query = query.Where(i => i.Status == request.Status.Value);
            }

            if (request.Year is not null)
            {
                query = query.Where(i => i.BillingYear == request.Year.Value);
            }

            if (request.Month is not null)
            {
                query = query.Where(i => i.BillingMonth == request.Month.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var term = request.Search.Trim();
                query = query.Where(i => i.Child.FullName.Contains(term)
                    || i.Child.Mother.FullName.Contains(term)
                    || i.Child.Father.FullName.Contains(term));
            }

            var count = await query.CountAsync(cancellationToken);

            var records = await query
                .OrderByDescending(i => i.BillingYear)
                .ThenByDescending(i => i.BillingMonth)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Include(i => i.Child).ThenInclude(c => c.Mother)
                .Include(i => i.Child).ThenInclude(c => c.Father)
                .Include(i => i.Child).ThenInclude(c => c.ParentUser)
                .Include(i => i.Child).ThenInclude(c => c.PlanAssignments).ThenInclude(a => a.Plan)
                .Include(i => i.MarkedPaidBy)
                .ToListAsync(cancellationToken);

            var items = records
                .Select(i => new InvoiceDto(
                    i.Id,
                    i.ChildId,
                    i.BillingMonth,
                    i.BillingYear,
                    i.PlanFee,
                    i.TotalOvertimeFee,
                    i.GrandTotal,
                    i.Status.ToString(),
                    i.PaidAt,
                    i.MarkedPaidById,
                    $"INV-{i.BillingYear:D4}-{i.BillingMonth:D2}-{i.Id.ToString("N")[..6].ToUpperInvariant()}",
                    i.Child.FullName,
                    i.ParentFullName ?? (i.Child.ParentUserId is not null ? i.Child.ParentUser!.FullName : i.Child.Mother.FullName),
                    i.ParentPhone ?? (i.Child.ParentUserId is not null ? i.Child.ParentUser!.PhoneNumber : i.Child.Mother.Phone),
                    i.Currency,
                    i.Status == InvoiceStatus.Paid ? i.GrandTotal : 0m,
                    i.Status == InvoiceStatus.Paid || i.Status == InvoiceStatus.Cancelled ? 0m : i.GrandTotal,
                    i.PlanName,
                    i.CreatedAt,
                    i.AdjustmentAmount,
                    i.AdjustmentReason,
                    i.PenaltyAmount,
                    i.LatePickupDays,
                    i.LatePickupFinePerDay,
                    i.OvertimeRate,
                    i.OvertimeHours,
                    i.PlanId,
                    new DateOnly(i.BillingYear, i.BillingMonth, 1).AddMonths(1).AddDays(4),
                    i.MarkedPaidBy != null ? i.MarkedPaidBy.FullName : null))
                .ToList();

            return new PaginatedList<InvoiceDto>(items, count, pageNumber, pageSize);
        }
    }
}
