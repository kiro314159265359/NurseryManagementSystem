using MediatR;
using NurseryManagementSystem.Application.Common.Exceptions;
using NurseryManagementSystem.Application.Common.Interfaces;
using NurseryManagementSystem.Domain.Entities.Plans;

namespace NurseryManagementSystem.Application.Features.Plans.Commands
{
    public record DeletePlanCommand(Guid Id) : IRequest<Unit>;

    public class DeletePlanCommandHandler : IRequestHandler<DeletePlanCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeletePlanCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Unit> Handle(DeletePlanCommand request, CancellationToken cancellationToken)
        {
            var repository = _unitOfWork.Repository<SubscriptionPlan>();

            var plan = await repository.GetByIdAsync(request.Id, cancellationToken);
            if (plan is null)
            {
                throw new NotFoundException("SubscriptionPlan", request.Id);
            }

            var isInUse = await _unitOfWork.Repository<ChildPlanAssignment>()
                .AnyAsync(a => a.PlanId == request.Id, cancellationToken);

            if (isInUse)
            {
                throw new ConflictException("This plan cannot be deleted because it is assigned to one or more children.");
            }

            repository.Remove(plan);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
