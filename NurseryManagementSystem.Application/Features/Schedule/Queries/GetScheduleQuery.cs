using MediatR;
using Microsoft.EntityFrameworkCore;
using NurseryManagementSystem.Application.Common.Interfaces;
using NurseryManagementSystem.Application.Features.Schedule.DTOs;
using NurseryManagementSystem.Domain.Entities.Schedule;

namespace NurseryManagementSystem.Application.Features.Schedule.Queries
{
    public record GetScheduleQuery(bool ActiveOnly = false) : IRequest<IReadOnlyList<ScheduleSlotDto>>;

    public class GetScheduleQueryHandler : IRequestHandler<GetScheduleQuery, IReadOnlyList<ScheduleSlotDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetScheduleQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IReadOnlyList<ScheduleSlotDto>> Handle(
            GetScheduleQuery request,
            CancellationToken cancellationToken)
        {
            var query = _unitOfWork.Repository<DailyScheduleSlot>().Query().AsNoTracking();

            if (request.ActiveOnly)
            {
                query = query.Where(s => s.IsActive);
            }

            return await query
                .OrderBy(s => s.SortOrder)
                .ThenBy(s => s.StartTime)
                .Select(s => new ScheduleSlotDto(
                    s.Id,
                    s.StartTime,
                    s.EndTime,
                    s.ActivityName,
                    s.Description,
                    s.SortOrder,
                    s.IsActive))
                .ToListAsync(cancellationToken);
        }
    }
}
