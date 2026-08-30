using MediatR;
using NurseryManagementSystem.Application.Common.Exceptions;
using NurseryManagementSystem.Application.Common.Interfaces;
using NurseryManagementSystem.Application.Features.Billing.DTOs;
using NurseryManagementSystem.Domain.Entities.Billing;
using Microsoft.EntityFrameworkCore;

namespace NurseryManagementSystem.Application.Features.Billing.Queries
{
    public record GetInvoiceByIdQuery(Guid Id) : IRequest<InvoiceDto>;

    public class GetInvoiceByIdQueryHandler : IRequestHandler<GetInvoiceByIdQuery, InvoiceDto>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetInvoiceByIdQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<InvoiceDto> Handle(GetInvoiceByIdQuery request, CancellationToken cancellationToken)
        {
            var invoice = await _unitOfWork.Repository<MonthlyInvoice>().Query()
                .AsNoTracking()
                .Include(i => i.Child).ThenInclude(c => c.Mother)
                .Include(i => i.Child).ThenInclude(c => c.ParentUser)
                .Include(i => i.MarkedPaidBy)
                .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);
            if (invoice is null)
            {
                throw new NotFoundException("MonthlyInvoice", request.Id);
            }

            return new InvoiceDto(
                invoice.Id,
                invoice.ChildId,
                invoice.BillingMonth,
                invoice.BillingYear,
                invoice.PlanFee,
                invoice.TotalOvertimeFee,
                invoice.GrandTotal,
                invoice.Status.ToString(),
                invoice.PaidAt,
                invoice.MarkedPaidById,
                $"INV-{invoice.BillingYear:D4}-{invoice.BillingMonth:D2}-{invoice.Id.ToString("N")[..6].ToUpperInvariant()}",
                invoice.Child.FullName,
                invoice.ParentFullName ?? invoice.Child.ParentUser?.FullName ?? invoice.Child.Mother.FullName,
                invoice.ParentPhone ?? invoice.Child.ParentUser?.PhoneNumber ?? invoice.Child.Mother.Phone,
                invoice.Currency,
                invoice.Status == Domain.Enums.InvoiceStatus.Paid ? invoice.GrandTotal : 0m,
                invoice.Status is Domain.Enums.InvoiceStatus.Paid or Domain.Enums.InvoiceStatus.Cancelled ? 0m : invoice.GrandTotal,
                invoice.PlanName,
                invoice.CreatedAt,
                invoice.AdjustmentAmount,
                invoice.AdjustmentReason,
                invoice.PenaltyAmount,
                invoice.LatePickupDays,
                invoice.LatePickupFinePerDay,
                invoice.OvertimeRate,
                invoice.OvertimeHours,
                invoice.PlanId,
                new DateOnly(invoice.BillingYear, invoice.BillingMonth, 1).AddMonths(1).AddDays(4),
                invoice.MarkedPaidBy?.FullName);
        }
    }
}
