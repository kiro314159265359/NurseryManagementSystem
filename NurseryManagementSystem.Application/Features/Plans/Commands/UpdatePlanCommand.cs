using FluentValidation;
using MediatR;
using NurseryManagementSystem.Application.Common.Exceptions;
using NurseryManagementSystem.Application.Common.Interfaces;
using NurseryManagementSystem.Domain.Entities.Plans;

namespace NurseryManagementSystem.Application.Features.Plans.Commands
{
    public record UpdatePlanCommand(
        Guid Id,
        string Name,
        int DurationHours,
        bool IsWeekend,
        decimal MonthlyFee,
        decimal DailyOvertimeFee) : IRequest<Unit>;

    public class UpdatePlanCommandValidator : AbstractValidator<UpdatePlanCommand>
    {
        public UpdatePlanCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
            RuleFor(x => x.DurationHours).GreaterThan(0);
            RuleFor(x => x.MonthlyFee).GreaterThanOrEqualTo(0);
            RuleFor(x => x.DailyOvertimeFee).GreaterThanOrEqualTo(0);
        }
    }

    public class UpdatePlanCommandHandler : IRequestHandler<UpdatePlanCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdatePlanCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Unit> Handle(UpdatePlanCommand request, CancellationToken cancellationToken)
        {
            var repository = _unitOfWork.Repository<SubscriptionPlan>();

            var plan = await repository.GetByIdAsync(request.Id, cancellationToken);
            if (plan is null)
            {
                throw new NotFoundException("SubscriptionPlan", request.Id);
            }

            plan.Name = request.Name;
            plan.DurationHours = request.DurationHours;
            plan.IsWeekend = request.IsWeekend;
            plan.MonthlyFee = request.MonthlyFee;
            plan.DailyOvertimeFee = request.DailyOvertimeFee;

            repository.Update(plan);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
