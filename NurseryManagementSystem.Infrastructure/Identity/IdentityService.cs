using Microsoft.AspNetCore.Identity;
using NurseryManagementSystem.Application.Common.Interfaces;
using NurseryManagementSystem.Domain.Entities.Identity;

namespace NurseryManagementSystem.Infrastructure.Identity
{
    public class IdentityService : IIdentityService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;

        public IdentityService(
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole<Guid>> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IQueryable<AppUser> Users => _userManager.Users;

        public async Task<AppUser?> FindByIdAsync(Guid userId, CancellationToken cancellationToken = default)
            => await _userManager.FindByIdAsync(userId.ToString());

        public async Task<AppUser?> FindByUserNameAsync(string userName, CancellationToken cancellationToken = default)
            => await _userManager.FindByNameAsync(userName);

        public async Task<bool> CheckPasswordAsync(AppUser user, string password)
            => await _userManager.CheckPasswordAsync(user, password);

        public async Task<IList<string>> GetRolesAsync(AppUser user)
            => await _userManager.GetRolesAsync(user);

        public async Task<(IdentityOperationResult Result, Guid UserId)> CreateUserAsync(
            AppUser user,
            string password,
            string role)
        {
            var createResult = await _userManager.CreateAsync(user, password);
            if (!createResult.Succeeded)
            {
                return (ToResult(createResult), Guid.Empty);
            }

            await EnsureRoleExistsAsync(role);
            var roleResult = await _userManager.AddToRoleAsync(user, role);

            return roleResult.Succeeded
                ? (IdentityOperationResult.Success(), user.Id)
                : (ToResult(roleResult), user.Id);
        }

        public async Task<IdentityOperationResult> UpdateUserAsync(AppUser user)
            => ToResult(await _userManager.UpdateAsync(user));

        public async Task<IdentityOperationResult> SetRoleAsync(AppUser user, string role)
        {
            await EnsureRoleExistsAsync(role);

            var currentRoles = await _userManager.GetRolesAsync(user);
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
            {
                return ToResult(removeResult);
            }

            return ToResult(await _userManager.AddToRoleAsync(user, role));
        }

        public async Task<IdentityOperationResult> ChangePasswordAsync(
            AppUser user,
            string currentPassword,
            string newPassword)
            => ToResult(await _userManager.ChangePasswordAsync(user, currentPassword, newPassword));

        private async Task EnsureRoleExistsAsync(string role)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole<Guid>(role));
            }
        }

        private static IdentityOperationResult ToResult(IdentityResult result)
            => result.Succeeded
                ? IdentityOperationResult.Success()
                : IdentityOperationResult.Failure(result.Errors.Select(e => e.Description).ToArray());
    }
}
