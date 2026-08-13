using MediatR;
using Microsoft.EntityFrameworkCore;
using NurseryManagementSystem.Application.Common.Interfaces;
using NurseryManagementSystem.Application.Common.Models;
using NurseryManagementSystem.Application.Features.Children.DTOs;
using NurseryManagementSystem.Domain.Entities.Children;

namespace NurseryManagementSystem.Application.Features.Children.Queries
{
    public record GetChildrenQuery(
        int PageNumber = 1,
        int PageSize = 20,
        string? Search = null,
        bool ActiveOnly = false) : IRequest<PaginatedList<ChildDto>>;

    public class GetChildrenQueryHandler : IRequestHandler<GetChildrenQuery, PaginatedList<ChildDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetChildrenQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PaginatedList<ChildDto>> Handle(GetChildrenQuery request, CancellationToken cancellationToken)
        {
            var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
            var pageSize = request.PageSize is < 1 or > 200 ? 20 : request.PageSize;

            var query = _unitOfWork.Repository<Child>().Query().AsNoTracking();

            if (request.ActiveOnly)
            {
                query = query.Where(c => c.IsActive);
            }

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var term = request.Search.Trim();
                query = query.Where(c => c.FullName.Contains(term) || c.QrCode.Contains(term));
            }

            var projected = query
                .OrderBy(c => c.FullName)
                .Select(c => new ChildDto(
                    c.Id,
                    c.FullName,
                    c.DateOfBirth,
                    c.EnrollmentDate,
                    c.Nationality,
                    c.Religion,
                    c.HomeAddress,
                    c.Allergies,
                    c.QrCode,
                    c.IsActive));

            return await PaginatedList<ChildDto>.CreateAsync(projected, pageNumber, pageSize, cancellationToken);
        }
    }
}
