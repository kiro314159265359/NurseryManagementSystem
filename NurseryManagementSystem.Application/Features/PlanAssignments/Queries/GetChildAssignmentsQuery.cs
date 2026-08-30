using MediatR;
using Microsoft.EntityFrameworkCore;
using NurseryManagementSystem.Application.Common.Interfaces;
using NurseryManagementSystem.Application.Features.PlanAssignments.DTOs;
using NurseryManagementSystem.Domain.Entities.Plans;

namespace NurseryManagementSystem.Application.Features.PlanAssignments.Queries
{
    public record GetChildAssignmentsQuery(Guid ChildId) : IRequest<IReadOnlyList<PlanAssignmentDto>>;

    public class GetChildAssignmentsQueryHandler
        : IRequestHandler<GetChildAssignmentsQuery, IReadOnlyList<PlanAssignmentDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetChildAssignmentsQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IReadOnlyList<PlanAssignmentDto>> Handle(
            GetChildAssignmentsQuery request,
            CancellationToken cancellationToken)
        {
            return await _unitOfWork.Repository<ChildPlanAssignment>().Query()
                .AsNoTracking()
                .Where(a => a.ChildId == request.ChildId)
                .OrderByDescending(a => a.StartDate)
                .Select(a => new PlanAssignmentDto(
                    a.Id,
                    a.ChildId,
                    a.PlanId,
                    a.PlanNameSnapshot == "" ? a.Plan.Name : a.PlanNameSnapshot,
                    a.PlanCategorySnapshot == "" ? a.Plan.Category : a.PlanCategorySnapshot,
                    a.PriceSnapshot == 0 ? a.Plan.MonthlyFee : a.PriceSnapshot,
                    a.DurationHoursSnapshot == 0 ? a.Plan.DurationHours : a.DurationHoursSnapshot,
                    a.DaysPerCycleSnapshot == 0 ? a.Plan.DaysPerCycle : a.DaysPerCycleSnapshot,
                    a.StartDate,
                    a.EndDate,
                    a.EndDate == null,
                    a.AssignedById,
                    a.AssignedBy.FullName,
                    a.AssignedAt,
                    a.CurrencySnapshot == "" ? a.Plan.Currency : a.CurrencySnapshot))
                .ToListAsync(cancellationToken);
        }
    }
}
