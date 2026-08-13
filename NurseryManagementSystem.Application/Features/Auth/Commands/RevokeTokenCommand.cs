using FluentValidation;
using MediatR;
using NurseryManagementSystem.Application.Common.Exceptions;
using NurseryManagementSystem.Application.Common.Interfaces;
using DomainRefreshToken = NurseryManagementSystem.Domain.Entities.Identity.RefreshToken;

namespace NurseryManagementSystem.Application.Features.Auth.Commands
{
    public record RevokeTokenCommand(string RefreshToken) : IRequest<Unit>;

    public class RevokeTokenCommandValidator : AbstractValidator<RevokeTokenCommand>
    {
        public RevokeTokenCommandValidator()
        {
            RuleFor(x => x.RefreshToken).NotEmpty();
        }
    }

    public class RevokeTokenCommandHandler : IRequestHandler<RevokeTokenCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;

        public RevokeTokenCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Unit> Handle(RevokeTokenCommand request, CancellationToken cancellationToken)
        {
            var repository = _unitOfWork.Repository<DomainRefreshToken>();

            var existing = await repository.FirstOrDefaultAsync(
                t => t.Token == request.RefreshToken,
                cancellationToken);

            if (existing is null)
            {
                throw new NotFoundException("RefreshToken", request.RefreshToken);
            }

            existing.IsRevoked = true;
            repository.Update(existing);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
