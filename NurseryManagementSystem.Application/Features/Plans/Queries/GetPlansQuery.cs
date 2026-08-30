using MediatR;
using Microsoft.EntityFrameworkCore;
using NurseryManagementSystem.Application.Common.Interfaces;
using NurseryManagementSystem.Application.Features.Plans.DTOs;
using NurseryManagementSystem.Domain.Entities.Plans;

namespace NurseryManagementSystem.Application.Features.Plans.Queries
{
    public record GetPlansQuery : IRequest<IReadOnlyList<PlanDto>>;

    public class GetPlansQueryHandler : IRequestHandler<GetPlansQuery, IReadOnlyList<PlanDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetPlansQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IReadOnlyList<PlanDto>> Handle(GetPlansQuery request, CancellationToken cancellationToken)
        {
            return await _unitOfWork.Repository<SubscriptionPlan>().Query()
                .AsNoTracking()
                .OrderBy(p => p.DisplayOrder).ThenBy(p => p.Name)
                .Select(p => new PlanDto(
                    p.Id,
                    p.Name,
                    p.DurationHours,
                    p.IsWeekend,
                    p.MonthlyFee,
                    p.DailyOvertimeFee,
                    p.Category,
                    p.BillingCycle,
                    p.DaysPerCycle,
                    p.IsFullDay,
                    p.BadgeText,
                    p.IsFeatured,
                    p.IsActive,
                    p.Currency,
                    p.DisplayOrder))
                .ToListAsync(cancellationToken);
        }
    }
}
