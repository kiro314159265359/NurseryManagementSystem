using FluentValidation;
using MediatR;
using NurseryManagementSystem.Application.Common.Exceptions;
using NurseryManagementSystem.Application.Common.Interfaces;
using NurseryManagementSystem.Application.Features.Auth.DTOs;
using NurseryManagementSystem.Domain.Entities.Identity;
using NurseryManagementSystem.Domain.Enums;

namespace NurseryManagementSystem.Application.Features.Auth.Commands
{
    public record LoginCommand(string UserName, string Password) : IRequest<AuthResponse>;

    public class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator()
        {
            RuleFor(x => x.UserName).NotEmpty().MaximumLength(256);
            RuleFor(x => x.Password).NotEmpty();
        }
    }

    public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponse>
    {
        private readonly IIdentityService _identityService;
        private readonly ITokenService _tokenService;
        private readonly IUnitOfWork _unitOfWork;

        public LoginCommandHandler(
            IIdentityService identityService,
            ITokenService tokenService,
            IUnitOfWork unitOfWork)
        {
            _identityService = identityService;
            _tokenService = tokenService;
            _unitOfWork = unitOfWork;
        }

        public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var user = await _identityService.FindByUserNameAsync(request.UserName, cancellationToken);

            if (user is null || !user.IsActive)
            {
                throw new UnauthorizedAccessException("Invalid credentials.");
            }

            if (!await _identityService.CheckPasswordAsync(user, request.Password))
            {
                throw new UnauthorizedAccessException("Invalid credentials.");
            }

            if (user.Role == UserRole.Parent && user.ApprovalStatus != ApprovalStatus.Approved)
            {
                var message = user.ApprovalStatus == ApprovalStatus.Pending
                    ? "Your account is waiting for admin approval."
                    : "Your registration was rejected. Contact the nursery for assistance.";
                throw new ForbiddenAccessException(message);
            }

            var roles = await _identityService.GetRolesAsync(user);

            var accessToken = _tokenService.CreateAccessToken(user, roles);
            var refreshTokenValue = _tokenService.CreateRefreshToken();
            var refreshExpiry = _tokenService.GetRefreshTokenExpiry();

            var refreshToken = new RefreshToken
            {
                Token = refreshTokenValue,
                ExpiresAt = refreshExpiry,
                IsRevoked = false,
                UserId = user.Id
            };

            await _unitOfWork.Repository<RefreshToken>().AddAsync(refreshToken, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new AuthResponse(
                user.Id,
                user.UserName ?? string.Empty,
                user.FullName,
                roles.FirstOrDefault() ?? user.Role.ToString(),
                accessToken,
                refreshTokenValue,
                refreshExpiry);
        }
    }
}
