using NurseryManagementSystem.Domain.Enums;

namespace NurseryManagementSystem.Application.Features.Registrations.DTOs
{
    public record RegistrationCreatedDto(
        Guid ParentUserId,
        Guid ChildId,
        ApprovalStatus ApprovalStatus);

    public record RegistrationDto(
        Guid ChildId,
        string ChildFullName,
        DateOnly DateOfBirth,
        DateOnly EnrollmentDate,
        ApprovalStatus ApprovalStatus,
        Guid ParentUserId,
        string ParentFullName,
        string ParentEmail,
        string ParentPhone,
        ParentRelationship AccountOwner,
        Guid? RequestedPlanId,
        string? RequestedPlanName,
        bool IsFirstChild,
        string? RejectionReason,
        DateTime SubmittedAt);
}
