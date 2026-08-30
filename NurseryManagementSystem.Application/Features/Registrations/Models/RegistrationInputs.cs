using NurseryManagementSystem.Application.Features.Children.Models;
using NurseryManagementSystem.Domain.Enums;

namespace NurseryManagementSystem.Application.Features.Registrations.Models
{
    public record ChildRegistrationInput(
        string FullName,
        DateOnly DateOfBirth,
        DateOnly EnrollmentDate,
        string Nationality,
        string Religion,
        string HomeAddress,
        string? Allergies,
        Guid? RequestedPlanId,
        ParentInput Mother,
        ParentInput Father,
        AgreementInput Agreement,
        IReadOnlyList<EmergencyContactInput> EmergencyContacts);

    public record FamilyRegistrationInput(
        ParentRelationship? AccountOwner,
        string? Password,
        ChildRegistrationInput Child);
}
