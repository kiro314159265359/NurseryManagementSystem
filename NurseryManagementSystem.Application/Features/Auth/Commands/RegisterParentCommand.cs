using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NurseryManagementSystem.Application.Common.Exceptions;
using NurseryManagementSystem.Application.Common.Interfaces;
using NurseryManagementSystem.Domain.Entities.Identity;
using NurseryManagementSystem.Domain.Enums;

namespace NurseryManagementSystem.Application.Features.Auth.Commands;

public record RegisterParentCommand(
    string FullName,
    string Email,
    string PhoneNumber,
    string Password) : IRequest<Guid>;

public class RegisterParentCommandValidator : AbstractValidator<RegisterParentCommand>
{
    public RegisterParentCommandValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.PhoneNumber).MaximumLength(30);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8).MaximumLength(128);
    }
}

public class RegisterParentCommandHandler : IRequestHandler<RegisterParentCommand, Guid>
{
    private readonly IIdentityService _identityService;

    public RegisterParentCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<Guid> Handle(RegisterParentCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        if (await _identityService.Users.AnyAsync(
                x => x.NormalizedEmail == normalizedEmail.ToUpper(), cancellationToken))
        {
            throw new ConflictException("An account with this email already exists.");
        }

        var user = new AppUser
        {
            UserName = normalizedEmail,
            Email = normalizedEmail,
            EmailConfirmed = true,
            PhoneNumber = request.PhoneNumber.Trim(),
            FullName = request.FullName.Trim(),
            Role = UserRole.Parent,
            IsActive = true
        };

        var (result, userId) = await _identityService.CreateUserAsync(
            user, request.Password, UserRole.Parent.ToString());

        if (!result.Succeeded)
        {
            throw new Common.Exceptions.ValidationException(
                result.Errors.Select(error => new ValidationFailure(nameof(request.Password), error)));
        }

        return userId;
    }
}
