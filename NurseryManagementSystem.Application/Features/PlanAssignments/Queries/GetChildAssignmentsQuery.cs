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
                    a.Plan.Name,
                    a.StartDate,
                    a.EndDate,
                    a.AssignedById))
                .ToListAsync(cancellationToken);
        }
    }
}
