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
            => RuleFor(command => command.Registration)
                .NotNull()
                .SetValidator(new FamilyRegistrationInputValidator());
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
            CancellationToken cancellationToken)
        {
            await RegistrationSupport.EnsurePlanExistsAsync(
                registration.Child, _unitOfWork, cancellationToken);

            var (fullName, email, phone) = RegistrationSupport.GetAccountOwner(registration);
            if (await _identityService.FindByUserNameAsync(email, cancellationToken) is not null)
            {
                throw new ConflictException($"An account with the email '{email}' already exists.");
            }

            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                var user = new AppUser
                {
                    UserName = email,
                    Email = email,
                    PhoneNumber = phone,
                    FullName = fullName,
                    Role = UserRole.Parent,
                    ParentRelationship = registration.AccountOwner,
                    ApprovalStatus = approvalStatus,
                    IsActive = true,
                    EmailConfirmed = true,
                    QrCode = $"PAR-{Guid.NewGuid():N}"
                };

                var (result, userId) = await _identityService.CreateUserAsync(
                    user, registration.Password, UserRole.Parent.ToString());
                if (!result.Succeeded)
                {
                    throw new ConflictException(string.Join("; ", result.Errors));
                }

                var child = RegistrationSupport.CreateChild(
                    registration.Child, userId, approvalStatus);
                await _unitOfWork.Repository<Child>().AddAsync(child, cancellationToken);

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

                return new RegistrationCreatedDto(userId, child.Id, approvalStatus);
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
            => CreateAsync(request.Registration, ApprovalStatus.Approved, cancellationToken);
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
            => CreateAsync(request.Registration, ApprovalStatus.Pending, cancellationToken);
    }
}
