using FluentValidation;
using MediatR;
using NurseryManagementSystem.Application.Common.Interfaces;
using NurseryManagementSystem.Application.Features.Auth.DTOs;
using DomainRefreshToken = NurseryManagementSystem.Domain.Entities.Identity.RefreshToken;

namespace NurseryManagementSystem.Application.Features.Auth.Commands
{
    public record RefreshTokenCommand(string RefreshToken) : IRequest<AuthResponse>;

    public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
    {
        public RefreshTokenCommandValidator()
        {
            RuleFor(x => x.RefreshToken).NotEmpty();
        }
    }

    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponse>
    {
        private readonly IIdentityService _identityService;
        private readonly ITokenService _tokenService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateTimeProvider _dateTimeProvider;

        public RefreshTokenCommandHandler(
            IIdentityService identityService,
            ITokenService tokenService,
            IUnitOfWork unitOfWork,
            IDateTimeProvider dateTimeProvider)
        {
            _identityService = identityService;
            _tokenService = tokenService;
            _unitOfWork = unitOfWork;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<AuthResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            var repository = _unitOfWork.Repository<DomainRefreshToken>();

            var existing = await repository.FirstOrDefaultAsync(
                t => t.Token == request.RefreshToken,
                cancellationToken);

            if (existing is null || existing.IsRevoked || existing.ExpiresAt <= _dateTimeProvider.UtcNow)
            {
                throw new UnauthorizedAccessException("Invalid or expired refresh token.");
            }

            var user = await _identityService.FindByIdAsync(existing.UserId, cancellationToken);
            if (user is null || !user.IsActive)
            {
                throw new UnauthorizedAccessException("Invalid credentials.");
            }

            existing.IsRevoked = true;
            repository.Update(existing);

            var roles = await _identityService.GetRolesAsync(user);

            var accessToken = _tokenService.CreateAccessToken(user, roles);
            var newRefreshValue = _tokenService.CreateRefreshToken();
            var refreshExpiry = _tokenService.GetRefreshTokenExpiry();

            var newRefreshToken = new DomainRefreshToken
            {
                Token = newRefreshValue,
                ExpiresAt = refreshExpiry,
                IsRevoked = false,
                UserId = user.Id
            };

            await repository.AddAsync(newRefreshToken, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new AuthResponse(
                user.Id,
                user.UserName ?? string.Empty,
                user.FullName,
                roles.FirstOrDefault() ?? user.Role.ToString(),
                accessToken,
                newRefreshValue,
                refreshExpiry);
        }
    }
}
