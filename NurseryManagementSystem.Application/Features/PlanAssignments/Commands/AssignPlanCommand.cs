using FluentValidation;
using MediatR;
using NurseryManagementSystem.Application.Common.Exceptions;
using NurseryManagementSystem.Application.Common.Interfaces;
using NurseryManagementSystem.Domain.Entities.Children;
using NurseryManagementSystem.Domain.Entities.Plans;
using NurseryManagementSystem.Domain.Enums;

namespace NurseryManagementSystem.Application.Features.PlanAssignments.Commands
{
    public record AssignPlanCommand(Guid ChildId, Guid PlanId, DateOnly StartDate) : IRequest<Guid>;

    public class AssignPlanCommandValidator : AbstractValidator<AssignPlanCommand>
    {
        public AssignPlanCommandValidator()
        {
            RuleFor(x => x.ChildId).NotEmpty();
            RuleFor(x => x.PlanId).NotEmpty();
            RuleFor(x => x.StartDate).NotEmpty();
        }
    }

    public class AssignPlanCommandHandler : IRequestHandler<AssignPlanCommand, Guid>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;

        public AssignPlanCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        public async Task<Guid> Handle(AssignPlanCommand request, CancellationToken cancellationToken)
        {
            var assignedById = _currentUser.UserId
                ?? throw new ForbiddenAccessException();

            var childExists = await _unitOfWork.Repository<Child>()
                .AnyAsync(c => c.Id == request.ChildId &&
                               c.ApprovalStatus == ApprovalStatus.Approved &&
                               c.IsActive,
                    cancellationToken);
            if (!childExists)
            {
                throw new NotFoundException("Child", request.ChildId);
            }

            var planExists = await _unitOfWork.Repository<SubscriptionPlan>()
                .AnyAsync(p => p.Id == request.PlanId, cancellationToken);
            if (!planExists)
            {
                throw new NotFoundException("SubscriptionPlan", request.PlanId);
            }

            var assignment = new ChildPlanAssignment
            {
                ChildId = request.ChildId,
                PlanId = request.PlanId,
                StartDate = request.StartDate,
                AssignedById = assignedById
            };

            await _unitOfWork.Repository<ChildPlanAssignment>().AddAsync(assignment, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return assignment.Id;
        }
    }
}
