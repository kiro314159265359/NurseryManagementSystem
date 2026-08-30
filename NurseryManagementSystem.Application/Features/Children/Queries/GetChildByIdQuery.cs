using MediatR;
using Microsoft.EntityFrameworkCore;
using NurseryManagementSystem.Application.Common.Exceptions;
using NurseryManagementSystem.Application.Common.Interfaces;
using NurseryManagementSystem.Application.Features.Children.DTOs;
using NurseryManagementSystem.Domain.Entities.Children;

namespace NurseryManagementSystem.Application.Features.Children.Queries
{
    public record GetChildByIdQuery(Guid Id) : IRequest<ChildDetailsDto>;

    public class GetChildByIdQueryHandler : IRequestHandler<GetChildByIdQuery, ChildDetailsDto>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetChildByIdQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ChildDetailsDto> Handle(GetChildByIdQuery request, CancellationToken cancellationToken)
        {
            var child = await _unitOfWork.Repository<Child>().Query()
                .AsNoTracking()
                .Include(c => c.Mother)
                .Include(c => c.Father)
                .Include(c => c.Agreement)
                .Include(c => c.EmergencyContacts)
                .Include(c => c.PlanAssignments).ThenInclude(a => a.Plan)
                .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

            if (child is null)
            {
                throw new NotFoundException("Child", request.Id);
            }

            return new ChildDetailsDto(
                child.Id,
                child.FullName,
                child.DateOfBirth,
                child.EnrollmentDate,
                child.Nationality,
                child.Religion,
                child.HomeAddress,
                child.Allergies,
                child.PhotoUrl,
                child.QrCode,
                child.IsActive,
                child.ApprovalStatus,
                child.ApprovalStatus != Domain.Enums.ApprovalStatus.Approved
                    ? child.ApprovalStatus.ToString()
                    : child.IsActive ? "Active" : "Inactive",
                child.CreatedAt,
                child.CreatedBy,
                child.ReviewedAt,
                child.ReviewedById,
                child.Mother is null
                    ? null
                    : new ParentDto(
                        child.Mother.FullName,
                        child.Mother.Phone,
                        child.Mother.Email,
                        child.Mother.Occupation,
                        child.Mother.JobTitle,
                        child.Mother.CompanyName,
                        child.Mother.WorkPhone,
                        child.Mother.Address),
                child.Father is null
                    ? null
                    : new ParentDto(
                        child.Father.FullName,
                        child.Father.Phone,
                        child.Father.Email,
                        child.Father.Occupation,
                        child.Father.JobTitle,
                        child.Father.CompanyName,
                        child.Father.WorkPhone,
                        child.Father.Address),
                child.Agreement is null
                    ? null
                    : new AgreementDto(
                        child.Agreement.MediaPermission,
                        child.Agreement.ParentSignature,
                        child.Agreement.SignedDate,
                        child.Agreement.AcceptedTerms),
                child.EmergencyContacts
                    .Select(e => new EmergencyContactDto(e.Id, e.Name, e.Relationship, e.Phone))
                    .ToList(),
                child.PlanAssignments
                    .Where(a => a.EndDate == null)
                    .OrderByDescending(a => a.StartDate)
                    .Select(a => new CurrentPlanDto(
                        a.Id, a.PlanId, a.Plan.Name, a.StartDate, a.Plan.DurationHours))
                    .FirstOrDefault());
        }
    }
}
