using MediatR;
using Microsoft.EntityFrameworkCore;
using NurseryManagementSystem.Application.Common.Interfaces;
using NurseryManagementSystem.Application.Common.Models;
using NurseryManagementSystem.Application.Features.Users.DTOs;

namespace NurseryManagementSystem.Application.Features.Users.Queries
{
    public record GetUsersQuery(int PageNumber = 1, int PageSize = 20, string? Search = null)
        : IRequest<PaginatedList<UserDto>>;

    public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, PaginatedList<UserDto>>
    {
        private readonly IIdentityService _identityService;

        public GetUsersQueryHandler(IIdentityService identityService)
        {
            _identityService = identityService;
        }

        public async Task<PaginatedList<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
        {
            var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
            var pageSize = request.PageSize is < 1 or > 200 ? 20 : request.PageSize;

            var query = _identityService.Users.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var term = request.Search.Trim();
                query = query.Where(u =>
                    (u.UserName != null && u.UserName.Contains(term)) ||
                    u.FullName.Contains(term));
            }

            var count = await query.CountAsync(cancellationToken);

            var users = await query
                .OrderBy(u => u.FullName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var items = users
                .Select(u => new UserDto(
                    u.Id,
                    u.UserName ?? string.Empty,
                    u.FullName,
                    u.Role.ToString(),
                    u.QrCode,
                    u.IsActive))
                .ToList();

            return new PaginatedList<UserDto>(items, count, pageNumber, pageSize);
        }
    }
}
