namespace NurseryManagementSystem.Application.Features.Children.Models
{
    public record ParentInput(
        string Phone,
        string Email,
        string Occupation,
        string JobTitle,
        string CompanyName,
        string WorkPhone,
        string Address);

    public record AgreementInput(
        bool MediaPermission,
        string ParentSignature,
        DateOnly SignedDate,
        bool AcceptedTerms);

    public record EmergencyContactInput(
        string Name,
        string Relationship,
        string Phone);
}
