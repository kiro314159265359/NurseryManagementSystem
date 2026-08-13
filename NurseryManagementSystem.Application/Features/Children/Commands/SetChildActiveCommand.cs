using MediatR;
using NurseryManagementSystem.Application.Common.Exceptions;
using NurseryManagementSystem.Application.Common.Interfaces;
using NurseryManagementSystem.Domain.Entities.Children;

namespace NurseryManagementSystem.Application.Features.Children.Commands
{
    public record SetChildActiveCommand(Guid Id, bool IsActive) : IRequest<Unit>;

    public class SetChildActiveCommandHandler : IRequestHandler<SetChildActiveCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;

        public SetChildActiveCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Unit> Handle(SetChildActiveCommand request, CancellationToken cancellationToken)
        {
            var repository = _unitOfWork.Repository<Child>();

            var child = await repository.GetByIdAsync(request.Id, cancellationToken);
            if (child is null)
            {
                throw new NotFoundException("Child", request.Id);
            }

            child.IsActive = request.IsActive;
            repository.Update(child);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
