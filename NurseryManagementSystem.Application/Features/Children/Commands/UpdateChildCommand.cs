using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NurseryManagementSystem.Application.Common.Exceptions;
using NurseryManagementSystem.Application.Common.Interfaces;
using NurseryManagementSystem.Application.Features.Children.Models;
using NurseryManagementSystem.Domain.Entities.Children;

namespace NurseryManagementSystem.Application.Features.Children.Commands
{
    public record UpdateChildCommand(
        Guid Id,
        string FullName,
        DateOnly DateOfBirth,
        DateOnly EnrollmentDate,
        string Nationality,
        string Religion,
        string HomeAddress,
        string? Allergies,
        ParentInput Mother,
        ParentInput Father,
        AgreementInput Agreement) : IRequest<Unit>;

    public class UpdateChildCommandValidator : AbstractValidator<UpdateChildCommand>
    {
        public UpdateChildCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Nationality).MaximumLength(100);
            RuleFor(x => x.Religion).MaximumLength(100);
            RuleFor(x => x.HomeAddress).MaximumLength(500);
            RuleFor(x => x.Allergies).MaximumLength(1000);
            RuleFor(x => x.Mother).NotNull();
            RuleFor(x => x.Father).NotNull();
            RuleFor(x => x.Agreement).NotNull();
        }
    }

    public class UpdateChildCommandHandler : IRequestHandler<UpdateChildCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdateChildCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Unit> Handle(UpdateChildCommand request, CancellationToken cancellationToken)
        {
            var repository = _unitOfWork.Repository<Child>();

            var child = await repository.Query()
                .Include(c => c.Mother)
                .Include(c => c.Father)
                .Include(c => c.Agreement)
                .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

            if (child is null)
            {
                throw new NotFoundException("Child", request.Id);
            }

            child.FullName = request.FullName;
            child.DateOfBirth = request.DateOfBirth;
            child.EnrollmentDate = request.EnrollmentDate;
            child.Nationality = request.Nationality;
            child.Religion = request.Religion;
            child.HomeAddress = request.HomeAddress;
            child.Allergies = request.Allergies;

            child.Mother ??= new Mother { ChildId = child.Id };
            child.Mother.FullName = request.Mother.FullName ?? string.Empty;
            child.Mother.Phone = request.Mother.Phone;
            child.Mother.Email = request.Mother.Email;
            child.Mother.Occupation = request.Mother.Occupation;
            child.Mother.JobTitle = request.Mother.JobTitle;
            child.Mother.CompanyName = request.Mother.CompanyName;
            child.Mother.WorkPhone = request.Mother.WorkPhone;
            child.Mother.Address = request.Mother.Address;

            child.Father ??= new Father { ChildId = child.Id };
            child.Father.FullName = request.Father.FullName ?? string.Empty;
            child.Father.Phone = request.Father.Phone;
            child.Father.Email = request.Father.Email;
            child.Father.Occupation = request.Father.Occupation;
            child.Father.JobTitle = request.Father.JobTitle;
            child.Father.CompanyName = request.Father.CompanyName;
            child.Father.WorkPhone = request.Father.WorkPhone;
            child.Father.Address = request.Father.Address;

            child.Agreement ??= new Agreement { ChildId = child.Id };
            child.Agreement.MediaPermission = request.Agreement.MediaPermission;
            child.Agreement.ParentSignature = request.Agreement.ParentSignature;
            child.Agreement.SignedDate = request.Agreement.SignedDate;
            child.Agreement.AcceptedTerms = request.Agreement.AcceptedTerms;

            repository.Update(child);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
