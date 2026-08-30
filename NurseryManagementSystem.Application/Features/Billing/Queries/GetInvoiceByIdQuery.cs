using MediatR;
using NurseryManagementSystem.Application.Common.Exceptions;
using NurseryManagementSystem.Application.Common.Interfaces;
using NurseryManagementSystem.Application.Features.Billing.DTOs;
using NurseryManagementSystem.Domain.Entities.Billing;

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
            var invoice = await _unitOfWork.Repository<MonthlyInvoice>().GetByIdAsync(request.Id, cancellationToken);
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
                null, null, null, "AED",
                invoice.Status == Domain.Enums.InvoiceStatus.Paid ? invoice.GrandTotal : 0m,
                invoice.Status is Domain.Enums.InvoiceStatus.Paid or Domain.Enums.InvoiceStatus.Cancelled ? 0m : invoice.GrandTotal,
                null,
                invoice.CreatedAt,
                invoice.AdjustmentAmount,
                invoice.AdjustmentReason,
                invoice.PenaltyAmount,
                invoice.LatePickupDays,
                invoice.LatePickupFinePerDay,
                invoice.OvertimeRate);
        }
    }
}
