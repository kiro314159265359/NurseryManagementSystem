using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NurseryManagementSystem.Application.Common.Exceptions;
using NurseryManagementSystem.Application.Common.Interfaces;
using NurseryManagementSystem.Domain.Entities.Children;
using NurseryManagementSystem.Domain.Entities.Plans;
using NurseryManagementSystem.Domain.Enums;

namespace NurseryManagementSystem.Application.Features.Registrations.Commands
{
    public record ReviewRegistrationCommand(
        Guid ChildId,
        bool Approve,
        string? RejectionReason = null) : IRequest<Unit>;

    public class ReviewRegistrationCommandValidator : AbstractValidator<ReviewRegistrationCommand>
    {
        public ReviewRegistrationCommandValidator()
        {
            RuleFor(command => command.ChildId).NotEmpty();
            RuleFor(command => command.RejectionReason)
                .NotEmpty()
                .MaximumLength(500)
                .When(command => !command.Approve);
        }
    }

    public class ReviewRegistrationCommandHandler
        : IRequestHandler<ReviewRegistrationCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IIdentityService _identityService;
        private readonly ICurrentUserService _currentUser;
        private readonly IDateTimeProvider _dateTime;

        public ReviewRegistrationCommandHandler(
            IUnitOfWork unitOfWork,
            IIdentityService identityService,
            ICurrentUserService currentUser,
            IDateTimeProvider dateTime)
        {
            _unitOfWork = unitOfWork;
            _identityService = identityService;
            _currentUser = currentUser;
            _dateTime = dateTime;
        }

        public async Task<Unit> Handle(
            ReviewRegistrationCommand request,
            CancellationToken cancellationToken)
        {
            var reviewerId = _currentUser.UserId ?? throw new ForbiddenAccessException();
            var childRepository = _unitOfWork.Repository<Child>();
            var child = await childRepository.Query()
                .FirstOrDefaultAsync(item => item.Id == request.ChildId, cancellationToken)
                ?? throw new NotFoundException("Child", request.ChildId);

            if (child.ApprovalStatus != ApprovalStatus.Pending)
            {
                throw new ConflictException("Only pending registrations can be reviewed.");
            }

            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                child.ApprovalStatus = request.Approve
                    ? ApprovalStatus.Approved
                    : ApprovalStatus.Rejected;
                child.IsActive = request.Approve;
                child.RejectionReason = request.Approve ? null : request.RejectionReason?.Trim();
                child.ReviewedAt = _dateTime.UtcNow;
                child.ReviewedById = reviewerId;

                if (request.Approve && child.RequestedPlanId is Guid planId)
                {
                    var alreadyAssigned = await _unitOfWork.Repository<ChildPlanAssignment>()
                        .AnyAsync(item => item.ChildId == child.Id && item.EndDate == null, cancellationToken);
                    if (!alreadyAssigned)
                    {
                        await _unitOfWork.Repository<ChildPlanAssignment>().AddAsync(
                            new ChildPlanAssignment
                            {
                                ChildId = child.Id,
                                PlanId = planId,
                                StartDate = child.EnrollmentDate,
                                AssignedById = reviewerId
                            }, cancellationToken);
                    }
                }

                if (child.ParentUserId is Guid parentUserId)
                {
                    var parent = await _identityService.FindByIdAsync(parentUserId, cancellationToken);
                    if (parent is not null && parent.Role == UserRole.Parent)
                    {
                        if (request.Approve)
                        {
                            parent.ApprovalStatus = ApprovalStatus.Approved;
                        }
                        else if (!await childRepository.Query().AnyAsync(
                            item => item.ParentUserId == parentUserId &&
                                    item.Id != child.Id &&
                                    item.ApprovalStatus != ApprovalStatus.Rejected,
                            cancellationToken))
                        {
                            parent.ApprovalStatus = ApprovalStatus.Rejected;
                        }

                        var updateResult = await _identityService.UpdateUserAsync(parent);
                        if (!updateResult.Succeeded)
                        {
                            throw new ConflictException(string.Join("; ", updateResult.Errors));
                        }
                    }
                }

                childRepository.Update(child);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);
                return Unit.Value;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }
    }
}
