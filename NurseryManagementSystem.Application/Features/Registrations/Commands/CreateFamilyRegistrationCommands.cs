using FluentValidation;
using MediatR;
using NurseryManagementSystem.Application.Common.Exceptions;
using NurseryManagementSystem.Application.Common.Interfaces;
using NurseryManagementSystem.Application.Features.Registrations.DTOs;
using NurseryManagementSystem.Application.Features.Registrations.Models;
using NurseryManagementSystem.Domain.Entities.Children;
using NurseryManagementSystem.Domain.Entities.Identity;
using NurseryManagementSystem.Domain.Entities.Plans;
using NurseryManagementSystem.Domain.Enums;

namespace NurseryManagementSystem.Application.Features.Registrations.Commands
{
    public record AdminCreateFamilyRegistrationCommand(FamilyRegistrationInput Registration)
        : IRequest<RegistrationCreatedDto>;

    public class AdminCreateFamilyRegistrationCommandValidator
        : AbstractValidator<AdminCreateFamilyRegistrationCommand>
    {
        public AdminCreateFamilyRegistrationCommandValidator()
        {
            RuleFor(command => command.Registration).NotNull();
            RuleFor(command => command.Registration.Child)
                .NotNull().SetValidator(new ChildRegistrationInputValidator());
            When(command => command.Registration.AccountOwner is not null, () =>
                RuleFor(command => command.Registration.AccountOwner).IsInEnum());
            When(command => command.Registration.AccountOwner is not null
                         && string.IsNullOrWhiteSpace(command.Registration.Password), () =>
                RuleFor(command => command.Registration.Password)
                    .Must(password => string.IsNullOrWhiteSpace(password) || password.Length >= 8)
                    .WithMessage("Password must be at least 8 characters when creating a new parent account."));
        }
    }

    public record SelfRegisterFamilyCommand(FamilyRegistrationInput Registration)
        : IRequest<RegistrationCreatedDto>;

    public class SelfRegisterFamilyCommandValidator : AbstractValidator<SelfRegisterFamilyCommand>
    {
        public SelfRegisterFamilyCommandValidator()
            => RuleFor(command => command.Registration)
                .NotNull()
                .SetValidator(new FamilyRegistrationInputValidator());
    }

    public abstract class CreateFamilyRegistrationHandler
    {
        private readonly IIdentityService _identityService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;

        protected CreateFamilyRegistrationHandler(
            IIdentityService identityService,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUser)
        {
            _identityService = identityService;
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        protected async Task<RegistrationCreatedDto> CreateAsync(
            FamilyRegistrationInput registration,
            ApprovalStatus approvalStatus,
            bool allowWithoutAccount,
            CancellationToken cancellationToken)
        {
            await RegistrationSupport.EnsurePlanExistsAsync(
                registration.Child, _unitOfWork, cancellationToken);

            var hasAccountOwner = registration.AccountOwner is not null;
            if (!hasAccountOwner && !allowWithoutAccount)
                throw new ConflictException("An account owner is required for self-registration.");
            var owner = hasAccountOwner
                ? RegistrationSupport.GetAccountOwner(registration)
                : (string.Empty, string.Empty, string.Empty);

            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                Guid? userId = null;
                if (hasAccountOwner)
                {
                    var existing = await _identityService.FindByUserNameAsync(owner.Item2, cancellationToken);
                    if (existing is not null)
                    {
                        if (existing.Role != UserRole.Parent)
                            throw new ConflictException("The selected email belongs to a non-parent account.");
                        userId = existing.Id;
                    }
                    else
                    {
                        if (string.IsNullOrWhiteSpace(registration.Password) || registration.Password.Length < 8)
                            throw new ConflictException("A password of at least 8 characters is required for a new parent account.");
                        var user = new AppUser
                        {
                            UserName = owner.Item2,
                            Email = owner.Item2,
                            PhoneNumber = owner.Item3,
                            FullName = owner.Item1,
                            Role = UserRole.Parent,
                            ParentRelationship = registration.AccountOwner,
                            ApprovalStatus = approvalStatus,
                            IsActive = true,
                            EmailConfirmed = true,
                            QrCode = $"PAR-{Guid.NewGuid():N}"
                        };
                        var (result, createdUserId) = await _identityService.CreateUserAsync(
                            user, registration.Password, UserRole.Parent.ToString());
                        if (!result.Succeeded)
                            throw new ConflictException(string.Join("; ", result.Errors));
                        userId = createdUserId;
                    }
                }

                var child = RegistrationSupport.CreateChild(
                    registration.Child, userId, approvalStatus);
                await _unitOfWork.Repository<Child>().AddAsync(child, cancellationToken);
                if (userId is Guid linkedUserId)
                {
                    await _unitOfWork.Repository<ParentChild>().AddAsync(new ParentChild
                    {
                        ParentUserId = linkedUserId,
                        ChildId = child.Id,
                        Relationship = registration.AccountOwner?.ToString() ?? "Parent"
                    }, cancellationToken);
                }

                if (approvalStatus == ApprovalStatus.Approved &&
                    child.RequestedPlanId is Guid planId)
                {
                    var assignedById = _currentUser.UserId
                        ?? throw new ForbiddenAccessException();
                    await _unitOfWork.Repository<ChildPlanAssignment>().AddAsync(
                        new ChildPlanAssignment
                        {
                            ChildId = child.Id,
                            PlanId = planId,
                            StartDate = child.EnrollmentDate,
                            AssignedById = assignedById
                        }, cancellationToken);
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                return new RegistrationCreatedDto(userId ?? Guid.Empty, child.Id, approvalStatus);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }
    }

    public class AdminCreateFamilyRegistrationCommandHandler
        : CreateFamilyRegistrationHandler,
          IRequestHandler<AdminCreateFamilyRegistrationCommand, RegistrationCreatedDto>
    {
        public AdminCreateFamilyRegistrationCommandHandler(
            IIdentityService identityService,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUser)
            : base(identityService, unitOfWork, currentUser)
        {
        }

        public Task<RegistrationCreatedDto> Handle(
            AdminCreateFamilyRegistrationCommand request,
            CancellationToken cancellationToken)
            => CreateAsync(request.Registration, ApprovalStatus.Approved, true, cancellationToken);
    }

    public class SelfRegisterFamilyCommandHandler
        : CreateFamilyRegistrationHandler,
          IRequestHandler<SelfRegisterFamilyCommand, RegistrationCreatedDto>
    {
        public SelfRegisterFamilyCommandHandler(
            IIdentityService identityService,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUser)
            : base(identityService, unitOfWork, currentUser)
        {
        }

        public Task<RegistrationCreatedDto> Handle(
            SelfRegisterFamilyCommand request,
            CancellationToken cancellationToken)
            => CreateAsync(request.Registration, ApprovalStatus.Pending, false, cancellationToken);
    }
}
