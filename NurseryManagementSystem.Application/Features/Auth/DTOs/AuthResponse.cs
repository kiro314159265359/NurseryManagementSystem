namespace NurseryManagementSystem.Application.Features.Auth.DTOs
{
    public record AuthResponse(
        Guid UserId,
        string UserName,
        string FullName,
        string Role,
        string AccessToken,
        string RefreshToken,
        DateTime RefreshTokenExpiresAt);
}
