using FluentValidation;
using MediatR;
using NurseryManagementSystem.Application.Common.Exceptions;
using NurseryManagementSystem.Application.Common.Interfaces;

namespace NurseryManagementSystem.Application.Features.Users.Commands
{
    public record ChangePasswordCommand(Guid Id, string CurrentPassword, string NewPassword) : IRequest<Unit>;

    public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
    {
        public ChangePasswordCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.CurrentPassword).NotEmpty();
            RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(8);
        }
    }

    public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Unit>
    {
        private readonly IIdentityService _identityService;

        public ChangePasswordCommandHandler(IIdentityService identityService)
        {
            _identityService = identityService;
        }

        public async Task<Unit> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
        {
            var user = await _identityService.FindByIdAsync(request.Id, cancellationToken);
            if (user is null)
            {
                throw new NotFoundException("User", request.Id);
            }

            var result = await _identityService.ChangePasswordAsync(
                user,
                request.CurrentPassword,
                request.NewPassword);

            if (!result.Succeeded)
            {
                throw new ConflictException(string.Join("; ", result.Errors));
            }

            return Unit.Value;
        }
    }
}
