using FluentValidation;
using MediatR;
using NurseryManagementSystem.Application.Common.Interfaces;
using NurseryManagementSystem.Application.Features.Children.Models;
using NurseryManagementSystem.Domain.Entities.Children;

namespace NurseryManagementSystem.Application.Features.Children.Commands
{
    public record CreateChildCommand(
        string FullName,
        DateOnly DateOfBirth,
        DateOnly EnrollmentDate,
        string Nationality,
        string Religion,
        string HomeAddress,
        string? Allergies,
        ParentInput Mother,
        ParentInput Father,
        AgreementInput Agreement,
        IReadOnlyList<EmergencyContactInput> EmergencyContacts) : IRequest<Guid>;

    public class CreateChildCommandValidator : AbstractValidator<CreateChildCommand>
    {
        public CreateChildCommandValidator()
        {
            RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Nationality).MaximumLength(100);
            RuleFor(x => x.Religion).MaximumLength(100);
            RuleFor(x => x.HomeAddress).MaximumLength(500);
            RuleFor(x => x.Allergies).MaximumLength(1000);
            RuleFor(x => x.Mother).NotNull();
            RuleFor(x => x.Father).NotNull();
            RuleFor(x => x.Agreement).NotNull();
            RuleForEach(x => x.EmergencyContacts).ChildRules(contact =>
            {
                contact.RuleFor(e => e.Name).NotEmpty().MaximumLength(150);
                contact.RuleFor(e => e.Relationship).MaximumLength(100);
                contact.RuleFor(e => e.Phone).MaximumLength(30);
            });
        }
    }

    public class CreateChildCommandHandler : IRequestHandler<CreateChildCommand, Guid>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CreateChildCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Guid> Handle(CreateChildCommand request, CancellationToken cancellationToken)
        {
            var child = new Child
            {
                FullName = request.FullName,
                DateOfBirth = request.DateOfBirth,
                EnrollmentDate = request.EnrollmentDate,
                Nationality = request.Nationality,
                Religion = request.Religion,
                HomeAddress = request.HomeAddress,
                Allergies = request.Allergies,
                QrCode = $"CHD-{Guid.NewGuid():N}",
                IsActive = true,
                Mother = new Mother
                {
                    FullName = request.Mother.FullName ?? string.Empty,
                    Phone = request.Mother.Phone,
                    Email = request.Mother.Email,
                    Occupation = request.Mother.Occupation,
                    JobTitle = request.Mother.JobTitle,
                    CompanyName = request.Mother.CompanyName,
                    WorkPhone = request.Mother.WorkPhone,
                    Address = request.Mother.Address
                },
                Father = new Father
                {
                    FullName = request.Father.FullName ?? string.Empty,
                    Phone = request.Father.Phone,
                    Email = request.Father.Email,
                    Occupation = request.Father.Occupation,
                    JobTitle = request.Father.JobTitle,
                    CompanyName = request.Father.CompanyName,
                    WorkPhone = request.Father.WorkPhone,
                    Address = request.Father.Address
                },
                Agreement = new Agreement
                {
                    MediaPermission = request.Agreement.MediaPermission,
                    ParentSignature = request.Agreement.ParentSignature,
                    SignedDate = request.Agreement.SignedDate,
                    AcceptedTerms = request.Agreement.AcceptedTerms
                }
            };

            foreach (var contact in request.EmergencyContacts)
            {
                child.EmergencyContacts.Add(new EmergencyContact
                {
                    Name = contact.Name,
                    Relationship = contact.Relationship,
                    Phone = contact.Phone
                });
            }

            await _unitOfWork.Repository<Child>().AddAsync(child, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return child.Id;
        }
    }
}
