using MediatR;
using NurseryManagementSystem.Application.Common.Exceptions;
using NurseryManagementSystem.Application.Common.Interfaces;
using NurseryManagementSystem.Domain.Entities.Billing;
using NurseryManagementSystem.Domain.Enums;

namespace NurseryManagementSystem.Application.Features.Billing.Commands
{
    public record MarkInvoicePaidCommand(Guid InvoiceId) : IRequest<Unit>;

    public class MarkInvoicePaidCommandHandler : IRequestHandler<MarkInvoicePaidCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;
        private readonly IDateTimeProvider _dateTime;

        public MarkInvoicePaidCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUser,
            IDateTimeProvider dateTime)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
            _dateTime = dateTime;
        }

        public async Task<Unit> Handle(MarkInvoicePaidCommand request, CancellationToken cancellationToken)
        {
            var repository = _unitOfWork.Repository<MonthlyInvoice>();

            var invoice = await repository.GetByIdAsync(request.InvoiceId, cancellationToken);
            if (invoice is null)
            {
                throw new NotFoundException("MonthlyInvoice", request.InvoiceId);
            }

            if (invoice.Status == InvoiceStatus.Paid)
            {
                throw new ConflictException("This invoice is already marked as paid.");
            }

            if (invoice.Status == InvoiceStatus.Cancelled)
            {
                throw new ConflictException("A cancelled invoice cannot be marked as paid.");
            }

            invoice.Status = InvoiceStatus.Paid;
            invoice.PaidAt = _dateTime.UtcNow;
            invoice.MarkedPaidById = _currentUser.UserId;

            repository.Update(invoice);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
