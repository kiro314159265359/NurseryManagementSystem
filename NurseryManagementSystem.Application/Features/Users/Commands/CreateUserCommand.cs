using FluentValidation;
using MediatR;
using NurseryManagementSystem.Application.Common.Exceptions;
using NurseryManagementSystem.Application.Common.Interfaces;
using NurseryManagementSystem.Domain.Entities.Identity;
using NurseryManagementSystem.Domain.Enums;

namespace NurseryManagementSystem.Application.Features.Users.Commands
{
    public record CreateUserCommand(
        string UserName,
        string FullName,
        string Password,
        UserRole Role) : IRequest<Guid>;

    public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
    {
        public CreateUserCommandValidator()
        {
            RuleFor(x => x.UserName).NotEmpty().MaximumLength(256);
            RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
            RuleFor(x => x.Role)
                .Must(role => role is UserRole.SuperAdmin or UserRole.SubAdmin)
                .WithMessage("Staff users must have the SuperAdmin or SubAdmin role.");
        }
    }

    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Guid>
    {
        private readonly IIdentityService _identityService;

        public CreateUserCommandHandler(IIdentityService identityService)
        {
            _identityService = identityService;
        }

        public async Task<Guid> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            var existing = await _identityService.FindByUserNameAsync(request.UserName, cancellationToken);
            if (existing is not null)
            {
                throw new ConflictException($"A user with the username '{request.UserName}' already exists.");
            }

            var user = new AppUser
            {
                UserName = request.UserName,
                FullName = request.FullName,
                Role = request.Role,
                IsActive = true,
                QrCode = $"STF-{Guid.NewGuid():N}"
            };

            var (result, userId) = await _identityService.CreateUserAsync(
                user,
                request.Password,
                request.Role.ToString());

            if (!result.Succeeded)
            {
                throw new ConflictException(string.Join("; ", result.Errors));
            }

            return userId;
        }
    }
}
