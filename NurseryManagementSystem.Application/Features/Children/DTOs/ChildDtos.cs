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
        string QrCode,
        bool IsActive,
        ApprovalStatus ApprovalStatus);

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
        string QrCode,
        bool IsActive,
        ApprovalStatus ApprovalStatus,
        ParentDto? Mother,
        ParentDto? Father,
        AgreementDto? Agreement,
        IReadOnlyList<EmergencyContactDto> EmergencyContacts);
}
