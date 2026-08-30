using FluentValidation;
using MediatR;
using NurseryManagementSystem.Application.Common.Exceptions;
using NurseryManagementSystem.Application.Common.Interfaces;
using NurseryManagementSystem.Application.Features.Registrations.DTOs;
using NurseryManagementSystem.Application.Features.Registrations.Models;
using NurseryManagementSystem.Domain.Entities.Children;
using NurseryManagementSystem.Domain.Enums;

namespace NurseryManagementSystem.Application.Features.Registrations.Commands
{
    public record SubmitChildRegistrationCommand(ChildRegistrationInput Child)
        : IRequest<RegistrationCreatedDto>;

    public class SubmitChildRegistrationCommandValidator
        : AbstractValidator<SubmitChildRegistrationCommand>
    {
        public SubmitChildRegistrationCommandValidator()
            => RuleFor(command => command.Child)
                .NotNull()
                .SetValidator(new ChildRegistrationInputValidator());
    }

    public class SubmitChildRegistrationCommandHandler
        : IRequestHandler<SubmitChildRegistrationCommand, RegistrationCreatedDto>
    {
        private readonly ICurrentUserService _currentUser;
        private readonly IIdentityService _identityService;
        private readonly IUnitOfWork _unitOfWork;

        public SubmitChildRegistrationCommandHandler(
            ICurrentUserService currentUser,
            IIdentityService identityService,
            IUnitOfWork unitOfWork)
        {
            _currentUser = currentUser;
            _identityService = identityService;
            _unitOfWork = unitOfWork;
        }

        public async Task<RegistrationCreatedDto> Handle(
            SubmitChildRegistrationCommand request,
            CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId ?? throw new ForbiddenAccessException();
            var user = await _identityService.FindByIdAsync(userId, cancellationToken)
                ?? throw new ForbiddenAccessException();
            if (user.Role != UserRole.Parent || user.ApprovalStatus != ApprovalStatus.Approved)
            {
                throw new ForbiddenAccessException();
            }

            await RegistrationSupport.EnsurePlanExistsAsync(
                request.Child, _unitOfWork, cancellationToken);

            var child = RegistrationSupport.CreateChild(
                request.Child, userId, ApprovalStatus.Pending);
            await _unitOfWork.Repository<Child>().AddAsync(child, cancellationToken);
            await _unitOfWork.Repository<ParentChild>().AddAsync(new ParentChild
            {
                ParentUserId = userId,
                ChildId = child.Id,
                Relationship = user.ParentRelationship?.ToString() ?? "Parent"
            }, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new RegistrationCreatedDto(userId, child.Id, ApprovalStatus.Pending);
        }
    }
}
