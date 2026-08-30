using FluentValidation;
using NurseryManagementSystem.Application.Common.Exceptions;
using NurseryManagementSystem.Application.Common.Interfaces;
using NurseryManagementSystem.Application.Features.Children.Models;
using NurseryManagementSystem.Application.Features.Registrations.Models;
using NurseryManagementSystem.Domain.Entities.Children;
using NurseryManagementSystem.Domain.Entities.Plans;
using NurseryManagementSystem.Domain.Enums;

namespace NurseryManagementSystem.Application.Features.Registrations
{
    internal static class RegistrationSupport
    {
        public static (string FullName, string Email, string Phone) GetAccountOwner(
            FamilyRegistrationInput registration)
        {
            var parent = registration.AccountOwner == ParentRelationship.Mother
                ? registration.Child.Mother
                : registration.Child.Father;

            return (
                parent.FullName?.Trim() ?? string.Empty,
                parent.Email.Trim(),
                parent.Phone.Trim());
        }

        public static Child CreateChild(
            ChildRegistrationInput input,
            Guid? parentUserId,
            ApprovalStatus approvalStatus)
        {
            return new Child
            {
                FullName = input.FullName.Trim(),
                DateOfBirth = input.DateOfBirth,
                EnrollmentDate = input.EnrollmentDate,
                Nationality = input.Nationality.Trim(),
                Religion = input.Religion.Trim(),
                HomeAddress = input.HomeAddress.Trim(),
                Allergies = input.Allergies?.Trim(),
                QrCode = $"CHD-{Guid.NewGuid():N}",
                IsActive = approvalStatus == ApprovalStatus.Approved,
                ParentUserId = parentUserId,
                ApprovalStatus = approvalStatus,
                RequestedPlanId = input.RequestedPlanId,
                Mother = CreateMother(input.Mother),
                Father = CreateFather(input.Father),
                Agreement = new Agreement
                {
                    MediaPermission = input.Agreement.MediaPermission,
                    ParentSignature = input.Agreement.ParentSignature.Trim(),
                    SignedDate = input.Agreement.SignedDate,
                    AcceptedTerms = input.Agreement.AcceptedTerms
                },
                EmergencyContacts = input.EmergencyContacts.Select(contact => new EmergencyContact
                {
                    Name = contact.Name.Trim(),
                    Relationship = contact.Relationship.Trim(),
                    Phone = contact.Phone.Trim()
                }).ToList()
            };
        }

        public static async Task EnsurePlanExistsAsync(
            ChildRegistrationInput child,
            IUnitOfWork unitOfWork,
            CancellationToken cancellationToken)
        {
            if (child.RequestedPlanId is not Guid planId)
            {
                return;
            }

            if (!await unitOfWork.Repository<SubscriptionPlan>()
                    .AnyAsync(plan => plan.Id == planId, cancellationToken))
            {
                throw new NotFoundException("SubscriptionPlan", planId);
            }
        }

        private static Mother CreateMother(ParentInput input) => new()
        {
            FullName = input.FullName?.Trim() ?? string.Empty,
            Phone = input.Phone.Trim(),
            Email = input.Email.Trim(),
            Occupation = input.Occupation.Trim(),
            JobTitle = input.JobTitle.Trim(),
            CompanyName = input.CompanyName.Trim(),
            WorkPhone = input.WorkPhone.Trim(),
            Address = input.Address.Trim()
        };

        private static Father CreateFather(ParentInput input) => new()
        {
            FullName = input.FullName?.Trim() ?? string.Empty,
            Phone = input.Phone.Trim(),
            Email = input.Email.Trim(),
            Occupation = input.Occupation.Trim(),
            JobTitle = input.JobTitle.Trim(),
            CompanyName = input.CompanyName.Trim(),
            WorkPhone = input.WorkPhone.Trim(),
            Address = input.Address.Trim()
        };
    }

    public class FamilyRegistrationInputValidator : AbstractValidator<FamilyRegistrationInput>
    {
        public FamilyRegistrationInputValidator()
        {
            RuleFor(input => input.AccountOwner).NotNull().IsInEnum();
            RuleFor(input => input.Password).NotEmpty().MinimumLength(8);
            RuleFor(input => input.Child).NotNull().SetValidator(new ChildRegistrationInputValidator());
        }
    }

    public class ChildRegistrationInputValidator : AbstractValidator<ChildRegistrationInput>
    {
        public ChildRegistrationInputValidator()
        {
            RuleFor(input => input.FullName).NotEmpty().MaximumLength(200);
            RuleFor(input => input.Nationality).MaximumLength(100);
            RuleFor(input => input.Religion).MaximumLength(100);
            RuleFor(input => input.HomeAddress).MaximumLength(500);
            RuleFor(input => input.Allergies).MaximumLength(1000);
            RuleFor(input => input.Mother).NotNull().SetValidator(new RegistrationParentInputValidator());
            RuleFor(input => input.Father).NotNull().SetValidator(new RegistrationParentInputValidator());
            RuleFor(input => input.Agreement).NotNull().SetValidator(new RegistrationAgreementInputValidator());
            RuleFor(input => input.EmergencyContacts).NotNull();
            RuleForEach(input => input.EmergencyContacts).ChildRules(contact =>
            {
                contact.RuleFor(item => item.Name).NotEmpty().MaximumLength(150);
                contact.RuleFor(item => item.Relationship).MaximumLength(100);
                contact.RuleFor(item => item.Phone).NotEmpty().MaximumLength(30);
            });
        }
    }

    public class RegistrationParentInputValidator : AbstractValidator<ParentInput>
    {
        public RegistrationParentInputValidator()
        {
            RuleFor(input => input.FullName).NotEmpty().MaximumLength(200);
            RuleFor(input => input.Email).NotEmpty().EmailAddress().MaximumLength(256);
            RuleFor(input => input.Phone).NotEmpty().MaximumLength(30);
            RuleFor(input => input.Occupation).MaximumLength(150);
            RuleFor(input => input.JobTitle).MaximumLength(150);
            RuleFor(input => input.CompanyName).MaximumLength(200);
            RuleFor(input => input.WorkPhone).MaximumLength(30);
            RuleFor(input => input.Address).MaximumLength(500);
        }
    }

    public class RegistrationAgreementInputValidator : AbstractValidator<AgreementInput>
    {
        public RegistrationAgreementInputValidator()
        {
            RuleFor(input => input.AcceptedTerms).Equal(true);
            RuleFor(input => input.ParentSignature).NotEmpty().MaximumLength(200);
        }
    }
}
