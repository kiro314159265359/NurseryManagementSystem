using FluentValidation;
using MediatR;
using NurseryManagementSystem.Application.Common.Exceptions;
using NurseryManagementSystem.Application.Common.Interfaces;
using NurseryManagementSystem.Domain.Entities.Plans;

namespace NurseryManagementSystem.Application.Features.PlanAssignments.Commands
{
    public record EndPlanAssignmentCommand(Guid AssignmentId, DateOnly EndDate) : IRequest<Unit>;

    public class EndPlanAssignmentCommandValidator : AbstractValidator<EndPlanAssignmentCommand>
    {
        public EndPlanAssignmentCommandValidator()
        {
            RuleFor(x => x.AssignmentId).NotEmpty();
            RuleFor(x => x.EndDate).NotEmpty();
        }
    }

    public class EndPlanAssignmentCommandHandler : IRequestHandler<EndPlanAssignmentCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;

        public EndPlanAssignmentCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Unit> Handle(EndPlanAssignmentCommand request, CancellationToken cancellationToken)
        {
            var repository = _unitOfWork.Repository<ChildPlanAssignment>();

            var assignment = await repository.GetByIdAsync(request.AssignmentId, cancellationToken);
            if (assignment is null)
            {
                throw new NotFoundException("ChildPlanAssignment", request.AssignmentId);
            }

            assignment.EndDate = request.EndDate;
            repository.Update(assignment);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
