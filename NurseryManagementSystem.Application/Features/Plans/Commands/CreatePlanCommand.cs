using FluentValidation;
using MediatR;
using NurseryManagementSystem.Application.Common.Interfaces;
using NurseryManagementSystem.Domain.Entities.Plans;

namespace NurseryManagementSystem.Application.Features.Plans.Commands
{
    public record CreatePlanCommand(
        string Name,
        int DurationHours,
        bool IsWeekend,
        decimal MonthlyFee,
        decimal DailyOvertimeFee) : IRequest<Guid>;

    public class CreatePlanCommandValidator : AbstractValidator<CreatePlanCommand>
    {
        public CreatePlanCommandValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
            RuleFor(x => x.DurationHours).GreaterThan(0);
            RuleFor(x => x.MonthlyFee).GreaterThanOrEqualTo(0);
            RuleFor(x => x.DailyOvertimeFee).GreaterThanOrEqualTo(0);
        }
    }

    public class CreatePlanCommandHandler : IRequestHandler<CreatePlanCommand, Guid>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CreatePlanCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Guid> Handle(CreatePlanCommand request, CancellationToken cancellationToken)
        {
            var plan = new SubscriptionPlan
            {
                Name = request.Name,
                DurationHours = request.DurationHours,
                IsWeekend = request.IsWeekend,
                MonthlyFee = request.MonthlyFee,
                DailyOvertimeFee = request.DailyOvertimeFee
            };

            await _unitOfWork.Repository<SubscriptionPlan>().AddAsync(plan, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return plan.Id;
        }
    }
}
