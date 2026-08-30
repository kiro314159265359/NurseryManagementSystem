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
        decimal DailyOvertimeFee,
        string Category = "Monthly Packages",
        string BillingCycle = "Monthly",
        int DaysPerCycle = 5,
        bool IsFullDay = false,
        string? BadgeText = null,
        bool IsFeatured = false,
        bool IsActive = true,
        string Currency = "AED",
        int DisplayOrder = 0) : IRequest<Guid>;

    public class CreatePlanCommandValidator : AbstractValidator<CreatePlanCommand>
    {
        public CreatePlanCommandValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
            RuleFor(x => x.DurationHours).GreaterThanOrEqualTo(0);
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
                DailyOvertimeFee = request.DailyOvertimeFee,
                Category = request.Category,
                BillingCycle = request.BillingCycle,
                DaysPerCycle = request.DaysPerCycle,
                IsFullDay = request.IsFullDay,
                BadgeText = request.BadgeText,
                IsFeatured = request.IsFeatured,
                IsActive = request.IsActive,
                Currency = request.Currency,
                DisplayOrder = request.DisplayOrder
            };

            await _unitOfWork.Repository<SubscriptionPlan>().AddAsync(plan, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return plan.Id;
        }
    }
}
