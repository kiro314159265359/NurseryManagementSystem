using FluentValidation;
using MediatR;
using NurseryManagementSystem.Application.Common.Exceptions;
using NurseryManagementSystem.Application.Common.Interfaces;
using NurseryManagementSystem.Domain.Enums;

namespace NurseryManagementSystem.Application.Features.Users.Commands
{
    public record AssignRoleCommand(Guid Id, UserRole Role) : IRequest<Unit>;

    public class AssignRoleCommandValidator : AbstractValidator<AssignRoleCommand>
    {
        public AssignRoleCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Role).IsInEnum();
        }
    }

    public class AssignRoleCommandHandler : IRequestHandler<AssignRoleCommand, Unit>
    {
        private readonly IIdentityService _identityService;

        public AssignRoleCommandHandler(IIdentityService identityService)
        {
            _identityService = identityService;
        }

        public async Task<Unit> Handle(AssignRoleCommand request, CancellationToken cancellationToken)
        {
            var user = await _identityService.FindByIdAsync(request.Id, cancellationToken);
            if (user is null)
            {
                throw new NotFoundException("User", request.Id);
            }

            user.Role = request.Role;

            var roleResult = await _identityService.SetRoleAsync(user, request.Role.ToString());
            if (!roleResult.Succeeded)
            {
                throw new ConflictException(string.Join("; ", roleResult.Errors));
            }

            var updateResult = await _identityService.UpdateUserAsync(user);
            if (!updateResult.Succeeded)
            {
                throw new ConflictException(string.Join("; ", updateResult.Errors));
            }

            return Unit.Value;
        }
    }
}
