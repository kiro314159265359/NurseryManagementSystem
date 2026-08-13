namespace NurseryManagementSystem.Application.Features.Users.DTOs
{
    public record UserDto(
        Guid Id,
        string UserName,
        string FullName,
        string Role,
        string? QrCode,
        bool IsActive);
}
