using NurseryManagementSystem.Domain.Enums;

namespace NurseryManagementSystem.Application.Features.Registrations.DTOs
{
    public record RegistrationCreatedDto(
        Guid ParentUserId,
        Guid ChildId,
        ApprovalStatus ApprovalStatus);

    public record RegistrationDto(
        Guid ChildId,
        string ChildName,
        DateOnly DateOfBirth,
        DateOnly EnrollmentDate,
        ApprovalStatus ApprovalStatus,
        Guid ParentUserId,
        string ParentName,
        string ParentEmail,
        string ParentPhone,
        ParentRelationship AccountOwner,
        Guid? RequestedPlanId,
        string? RejectionReason,
        DateTime CreatedAt);
}
