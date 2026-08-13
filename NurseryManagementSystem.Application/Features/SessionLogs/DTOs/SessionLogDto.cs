namespace NurseryManagementSystem.Application.Features.SessionLogs.DTOs
{
    public record SessionLogDto(
        Guid Id,
        Guid UserId,
        DateTime LoginAt,
        DateTime? LogoutAt,
        string IpAddress);
}
