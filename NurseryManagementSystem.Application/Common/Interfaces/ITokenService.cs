using NurseryManagementSystem.Domain.Entities.Identity;

namespace NurseryManagementSystem.Application.Common.Interfaces
{
    public interface ITokenService
    {
        string CreateAccessToken(AppUser user, IEnumerable<string> roles);

        string CreateRefreshToken();

        DateTime GetRefreshTokenExpiry();
    }
}
