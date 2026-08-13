using MediatR;
using NurseryManagementSystem.Application.Common.Exceptions;
using NurseryManagementSystem.Application.Common.Interfaces;

namespace NurseryManagementSystem.Application.Features.Users.Commands
{
    public record SetUserActiveCommand(Guid Id, bool IsActive) : IRequest<Unit>;

    public class SetUserActiveCommandHandler : IRequestHandler<SetUserActiveCommand, Unit>
    {
        private readonly IIdentityService _identityService;

        public SetUserActiveCommandHandler(IIdentityService identityService)
        {
            _identityService = identityService;
        }

        public async Task<Unit> Handle(SetUserActiveCommand request, CancellationToken cancellationToken)
        {
            var user = await _identityService.FindByIdAsync(request.Id, cancellationToken);
            if (user is null)
            {
                throw new NotFoundException("User", request.Id);
            }

            user.IsActive = request.IsActive;

            var result = await _identityService.UpdateUserAsync(user);
            if (!result.Succeeded)
            {
                throw new ConflictException(string.Join("; ", result.Errors));
            }

            return Unit.Value;
        }
    }
}
