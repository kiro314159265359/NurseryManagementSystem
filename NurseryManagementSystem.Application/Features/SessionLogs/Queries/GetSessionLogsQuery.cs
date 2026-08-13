using MediatR;
using Microsoft.EntityFrameworkCore;
using NurseryManagementSystem.Application.Common.Interfaces;
using NurseryManagementSystem.Application.Common.Models;
using NurseryManagementSystem.Application.Features.SessionLogs.DTOs;
using NurseryManagementSystem.Domain.Entities.Attendance;

namespace NurseryManagementSystem.Application.Features.SessionLogs.Queries
{
    public record GetSessionLogsQuery(
        Guid? UserId = null,
        int PageNumber = 1,
        int PageSize = 20) : IRequest<PaginatedList<SessionLogDto>>;

    public class GetSessionLogsQueryHandler : IRequestHandler<GetSessionLogsQuery, PaginatedList<SessionLogDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetSessionLogsQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PaginatedList<SessionLogDto>> Handle(
            GetSessionLogsQuery request,
            CancellationToken cancellationToken)
        {
            var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
            var pageSize = request.PageSize is < 1 or > 200 ? 20 : request.PageSize;

            var query = _unitOfWork.Repository<SessionLog>().Query().AsNoTracking();

            if (request.UserId is not null)
            {
                query = query.Where(s => s.UserId == request.UserId.Value);
            }

            var projected = query
                .OrderByDescending(s => s.LoginAt)
                .Select(s => new SessionLogDto(
                    s.Id,
                    s.UserId,
                    s.LoginAt,
                    s.LogoutAt,
                    s.IpAddress));

            return await PaginatedList<SessionLogDto>.CreateAsync(projected, pageNumber, pageSize, cancellationToken);
        }
    }
}
