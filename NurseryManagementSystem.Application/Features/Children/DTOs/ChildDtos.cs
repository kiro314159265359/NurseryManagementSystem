using NurseryManagementSystem.Domain.Enums;

namespace NurseryManagementSystem.Application.Features.Children.DTOs
{
    public record ChildDto(
        Guid Id,
        string FullName,
        DateOnly DateOfBirth,
        DateOnly EnrollmentDate,
        string Nationality,
        string Religion,
        string HomeAddress,
        string? Allergies,
        string? PhotoUrl,
        string ScanCode,
        bool IsActive,
        ApprovalStatus ApprovalStatus,
        string Status,
        DateTime CreatedAt,
        CurrentPlanDto? CurrentPlan);

    public record CurrentPlanDto(
        Guid AssignmentId,
        Guid PlanId,
        string PlanName,
        DateOnly StartDate,
        int DurationHours);

    public record ParentDto(
        string FullName,
        string Phone,
        string Email,
        string Occupation,
        string JobTitle,
        string CompanyName,
        string WorkPhone,
        string Address);

    public record AgreementDto(
        bool MediaPermission,
        string ParentSignature,
        DateOnly SignedDate,
        bool AcceptedTerms);

    public record EmergencyContactDto(
        Guid Id,
        string Name,
        string Relationship,
        string Phone);

    public record ChildDetailsDto(
        Guid Id,
        string FullName,
        DateOnly DateOfBirth,
        DateOnly EnrollmentDate,
        string Nationality,
        string Religion,
        string HomeAddress,
        string? Allergies,
        string? PhotoUrl,
        string ScanCode,
        bool IsActive,
        ApprovalStatus ApprovalStatus,
        string Status,
        DateTime CreatedAt,
        Guid? CreatedBy,
        DateTime? ApprovedAt,
        Guid? ApprovedBy,
        ParentDto? Mother,
        ParentDto? Father,
        AgreementDto? Agreement,
        IReadOnlyList<EmergencyContactDto> EmergencyContacts,
        CurrentPlanDto? CurrentPlan);
}
