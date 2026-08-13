using MediatR;
using NurseryManagementSystem.Application.Common.Exceptions;
using NurseryManagementSystem.Application.Common.Interfaces;
using NurseryManagementSystem.Application.Features.Users.DTOs;

namespace NurseryManagementSystem.Application.Features.Users.Queries
{
    public record GetUserByIdQuery(Guid Id) : IRequest<UserDto>;

    public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto>
    {
        private readonly IIdentityService _identityService;

        public GetUserByIdQueryHandler(IIdentityService identityService)
        {
            _identityService = identityService;
        }

        public async Task<UserDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            var user = await _identityService.FindByIdAsync(request.Id, cancellationToken);
            if (user is null)
            {
                throw new NotFoundException("User", request.Id);
            }

            return new UserDto(
                user.Id,
                user.UserName ?? string.Empty,
                user.FullName,
                user.Role.ToString(),
                user.QrCode,
                user.IsActive);
        }
    }
}
