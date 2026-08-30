using MediatR;
using Microsoft.EntityFrameworkCore;
using NurseryManagementSystem.Application.Common.Exceptions;
using NurseryManagementSystem.Application.Common.Interfaces;
using NurseryManagementSystem.Application.Features.Registrations.DTOs;
using NurseryManagementSystem.Domain.Entities.Children;
using NurseryManagementSystem.Domain.Enums;

namespace NurseryManagementSystem.Application.Features.Registrations.Queries
{
    public record GetPendingRegistrationsQuery : IRequest<IReadOnlyList<RegistrationDto>>;
    public record GetMyRegistrationsQuery : IRequest<IReadOnlyList<RegistrationDto>>;

    public class GetPendingRegistrationsQueryHandler
        : IRequestHandler<GetPendingRegistrationsQuery, IReadOnlyList<RegistrationDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetPendingRegistrationsQueryHandler(IUnitOfWork unitOfWork)
            => _unitOfWork = unitOfWork;

        public async Task<IReadOnlyList<RegistrationDto>> Handle(
            GetPendingRegistrationsQuery request,
            CancellationToken cancellationToken)
        {
            var children = await RegistrationProjection(_unitOfWork)
                .Where(child => child.ApprovalStatus == ApprovalStatus.Pending)
                .OrderBy(child => child.CreatedAt)
                .ToListAsync(cancellationToken);
            return children.Select(ToDto).ToList();
        }

        internal static IQueryable<Child> RegistrationProjection(IUnitOfWork unitOfWork)
            => unitOfWork.Repository<Child>().Query()
                .AsNoTracking()
                .Include(child => child.ParentUser)
                .Include(child => child.Mother)
                .Include(child => child.Father);

        internal static RegistrationDto ToDto(Child child)
        {
            var fatherOwnsAccount = child.ParentUser?.ParentRelationship == ParentRelationship.Father;
            var ownerName = fatherOwnsAccount ? child.Father?.FullName : child.Mother?.FullName;
            var ownerEmail = fatherOwnsAccount ? child.Father?.Email : child.Mother?.Email;
            var ownerPhone = fatherOwnsAccount ? child.Father?.Phone : child.Mother?.Phone;

            return new RegistrationDto(
                child.Id,
                child.FullName,
                child.DateOfBirth,
                child.EnrollmentDate,
                child.ApprovalStatus,
                child.ParentUserId ?? Guid.Empty,
                child.ParentUser?.FullName ?? ownerName ?? string.Empty,
                child.ParentUser?.Email ?? ownerEmail ?? string.Empty,
                child.ParentUser?.PhoneNumber ?? ownerPhone ?? string.Empty,
                child.ParentUser?.ParentRelationship ?? ParentRelationship.Mother,
                child.RequestedPlanId,
                child.RejectionReason,
                child.CreatedAt);
        }
    }

    public class GetMyRegistrationsQueryHandler
        : IRequestHandler<GetMyRegistrationsQuery, IReadOnlyList<RegistrationDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;

        public GetMyRegistrationsQueryHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUser)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        public async Task<IReadOnlyList<RegistrationDto>> Handle(
            GetMyRegistrationsQuery request,
            CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId ?? throw new ForbiddenAccessException();
            var children = await GetPendingRegistrationsQueryHandler.RegistrationProjection(_unitOfWork)
                .Where(child => child.ParentUserId == userId)
                .OrderByDescending(child => child.CreatedAt)
                .ToListAsync(cancellationToken);
            return children.Select(GetPendingRegistrationsQueryHandler.ToDto).ToList();
        }
    }
}
