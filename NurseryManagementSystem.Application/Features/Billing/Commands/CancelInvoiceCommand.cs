using MediatR;
using NurseryManagementSystem.Application.Common.Exceptions;
using NurseryManagementSystem.Application.Common.Interfaces;
using NurseryManagementSystem.Domain.Entities.Billing;
using NurseryManagementSystem.Domain.Enums;

namespace NurseryManagementSystem.Application.Features.Billing.Commands
{
    public record CancelInvoiceCommand(Guid InvoiceId) : IRequest<Unit>;

    public class CancelInvoiceCommandHandler : IRequestHandler<CancelInvoiceCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CancelInvoiceCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Unit> Handle(CancelInvoiceCommand request, CancellationToken cancellationToken)
        {
            var repository = _unitOfWork.Repository<MonthlyInvoice>();

            var invoice = await repository.GetByIdAsync(request.InvoiceId, cancellationToken);
            if (invoice is null)
            {
                throw new NotFoundException("MonthlyInvoice", request.InvoiceId);
            }

            if (invoice.Status == InvoiceStatus.Paid)
            {
                throw new ConflictException("A paid invoice cannot be cancelled.");
            }

            invoice.Status = InvoiceStatus.Cancelled;
            repository.Update(invoice);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
