using FluentValidation;
using MediatR;
using NurseryManagementSystem.Application.Common.Exceptions;
using NurseryManagementSystem.Application.Common.Interfaces;

namespace NurseryManagementSystem.Application.Features.Users.Commands
{
    public record UpdateUserCommand(Guid Id, string FullName) : IRequest<Unit>;

    public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
    {
        public UpdateUserCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        }
    }

    public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Unit>
    {
        private readonly IIdentityService _identityService;

        public UpdateUserCommandHandler(IIdentityService identityService)
        {
            _identityService = identityService;
        }

        public async Task<Unit> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _identityService.FindByIdAsync(request.Id, cancellationToken);
            if (user is null)
            {
                throw new NotFoundException("User", request.Id);
            }

            user.FullName = request.FullName;

            var result = await _identityService.UpdateUserAsync(user);
            if (!result.Succeeded)
            {
                throw new ConflictException(string.Join("; ", result.Errors));
            }

            return Unit.Value;
        }
    }
}
