using NurseryManagementSystem.Domain.Entities.Identity;

namespace NurseryManagementSystem.Application.Common.Interfaces
{
    public record IdentityOperationResult(bool Succeeded, string[] Errors)
    {
        public static IdentityOperationResult Success() => new(true, Array.Empty<string>());

        public static IdentityOperationResult Failure(params string[] errors) => new(false, errors);
    }

    public interface IIdentityService
    {
        IQueryable<AppUser> Users { get; }

        Task<AppUser?> FindByIdAsync(Guid userId, CancellationToken cancellationToken = default);

        Task<AppUser?> FindByUserNameAsync(string userName, CancellationToken cancellationToken = default);

        Task<bool> CheckPasswordAsync(AppUser user, string password);

        Task<IList<string>> GetRolesAsync(AppUser user);

        Task<(IdentityOperationResult Result, Guid UserId)> CreateUserAsync(
            AppUser user,
            string password,
            string role);

        Task<IdentityOperationResult> UpdateUserAsync(AppUser user);

        Task<IdentityOperationResult> SetRoleAsync(AppUser user, string role);

        Task<IdentityOperationResult> ChangePasswordAsync(
            AppUser user,
            string currentPassword,
            string newPassword);
    }
}
