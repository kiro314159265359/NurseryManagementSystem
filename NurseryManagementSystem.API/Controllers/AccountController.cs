using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NurseryManagementSystem.Application.Common.Interfaces;

namespace NurseryManagementSystem.API.Controllers;

[Authorize]
public class AccountController : ApiControllerBase
{
    private readonly ICurrentUserService _currentUser;
    private readonly IIdentityService _identityService;

    public AccountController(ICurrentUserService currentUser, IIdentityService identityService)
    {
        _currentUser = currentUser;
        _identityService = identityService;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMe(CancellationToken cancellationToken)
    {
        var user = await GetCurrentUser(cancellationToken);
        var roles = await _identityService.GetRolesAsync(user);
        return Ok(new
        {
            user.Id,
            user.FullName,
            user.UserName,
            user.Email,
            user.PhoneNumber,
            Role = roles.FirstOrDefault() ?? user.Role.ToString()
        });
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateMe(UpdateMyProfileRequest request, CancellationToken cancellationToken)
    {
        var user = await GetCurrentUser(cancellationToken);
        user.FullName = request.FullName.Trim();
        user.PhoneNumber = request.PhoneNumber?.Trim();

        var result = await _identityService.UpdateUserAsync(user);
        if (!result.Succeeded)
        {
            return ValidationProblem(new ValidationProblemDetails(
                new Dictionary<string, string[]> { ["profile"] = result.Errors }));
        }

        return NoContent();
    }

    [HttpPut("password")]
    public async Task<IActionResult> ChangePassword(ChangeMyPasswordRequest request, CancellationToken cancellationToken)
    {
        var user = await GetCurrentUser(cancellationToken);
        var result = await _identityService.ChangePasswordAsync(
            user, request.CurrentPassword, request.NewPassword);

        if (!result.Succeeded)
        {
            return ValidationProblem(new ValidationProblemDetails(
                new Dictionary<string, string[]> { ["password"] = result.Errors }));
        }

        return NoContent();
    }

    private async Task<NurseryManagementSystem.Domain.Entities.Identity.AppUser> GetCurrentUser(
        CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not Guid userId)
        {
            throw new UnauthorizedAccessException();
        }

        return await _identityService.FindByIdAsync(userId, cancellationToken)
            ?? throw new UnauthorizedAccessException();
    }
}

public record UpdateMyProfileRequest(string FullName, string? PhoneNumber);
public record ChangeMyPasswordRequest(string CurrentPassword, string NewPassword);
