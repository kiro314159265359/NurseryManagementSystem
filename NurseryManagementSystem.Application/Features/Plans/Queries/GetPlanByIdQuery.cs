using MediatR;
using NurseryManagementSystem.Application.Common.Exceptions;
using NurseryManagementSystem.Application.Common.Interfaces;
using NurseryManagementSystem.Application.Features.Plans.DTOs;
using NurseryManagementSystem.Domain.Entities.Plans;

namespace NurseryManagementSystem.Application.Features.Plans.Queries
{
    public record GetPlanByIdQuery(Guid Id) : IRequest<PlanDto>;

    public class GetPlanByIdQueryHandler : IRequestHandler<GetPlanByIdQuery, PlanDto>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetPlanByIdQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PlanDto> Handle(GetPlanByIdQuery request, CancellationToken cancellationToken)
        {
            var plan = await _unitOfWork.Repository<SubscriptionPlan>().GetByIdAsync(request.Id, cancellationToken);
            if (plan is null)
            {
                throw new NotFoundException("SubscriptionPlan", request.Id);
            }

            return new PlanDto(
                plan.Id,
                plan.Name,
                plan.DurationHours,
                plan.IsWeekend,
                plan.MonthlyFee,
                plan.DailyOvertimeFee);
        }
    }
}
