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
        decimal DailyOvertimeFee,
        string Category = "Monthly Packages",
        string BillingCycle = "Monthly",
        int DaysPerCycle = 5,
        bool IsFullDay = false,
        string? BadgeText = null,
        bool IsFeatured = false,
        bool IsActive = true,
        string Currency = "AED",
        int DisplayOrder = 0) : IRequest<Unit>;

    public class UpdatePlanCommandValidator : AbstractValidator<UpdatePlanCommand>
    {
        public UpdatePlanCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
            RuleFor(x => x.DurationHours).GreaterThanOrEqualTo(0);
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
            plan.Category = request.Category;
            plan.BillingCycle = request.BillingCycle;
            plan.DaysPerCycle = request.DaysPerCycle;
            plan.IsFullDay = request.IsFullDay;
            plan.BadgeText = request.BadgeText;
            plan.IsFeatured = request.IsFeatured;
            plan.IsActive = request.IsActive;
            plan.Currency = request.Currency;
            plan.DisplayOrder = request.DisplayOrder;

            repository.Update(plan);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
