using MediatR;
using NurseryManagementSystem.Application.Common.Exceptions;
using NurseryManagementSystem.Application.Common.Interfaces;
using NurseryManagementSystem.Domain.Entities.Children;

namespace NurseryManagementSystem.Application.Features.Children.Commands
{
    public record RemoveEmergencyContactCommand(Guid ChildId, Guid ContactId) : IRequest<Unit>;

    public class RemoveEmergencyContactCommandHandler : IRequestHandler<RemoveEmergencyContactCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;

        public RemoveEmergencyContactCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Unit> Handle(RemoveEmergencyContactCommand request, CancellationToken cancellationToken)
        {
            var repository = _unitOfWork.Repository<EmergencyContact>();

            var contact = await repository.FirstOrDefaultAsync(
                e => e.Id == request.ContactId && e.ChildId == request.ChildId,
                cancellationToken);

            if (contact is null)
            {
                throw new NotFoundException("EmergencyContact", request.ContactId);
            }

            repository.Remove(contact);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
