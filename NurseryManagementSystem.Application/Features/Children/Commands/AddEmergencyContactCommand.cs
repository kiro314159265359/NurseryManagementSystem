using FluentValidation;
using MediatR;
using NurseryManagementSystem.Application.Common.Exceptions;
using NurseryManagementSystem.Application.Common.Interfaces;
using NurseryManagementSystem.Domain.Entities.Children;

namespace NurseryManagementSystem.Application.Features.Children.Commands
{
    public record AddEmergencyContactCommand(
        Guid ChildId,
        string Name,
        string Relationship,
        string Phone) : IRequest<Guid>;

    public class AddEmergencyContactCommandValidator : AbstractValidator<AddEmergencyContactCommand>
    {
        public AddEmergencyContactCommandValidator()
        {
            RuleFor(x => x.ChildId).NotEmpty();
            RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
            RuleFor(x => x.Relationship).MaximumLength(100);
            RuleFor(x => x.Phone).MaximumLength(30);
        }
    }

    public class AddEmergencyContactCommandHandler : IRequestHandler<AddEmergencyContactCommand, Guid>
    {
        private readonly IUnitOfWork _unitOfWork;

        public AddEmergencyContactCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Guid> Handle(AddEmergencyContactCommand request, CancellationToken cancellationToken)
        {
            var childExists = await _unitOfWork.Repository<Child>()
                .AnyAsync(c => c.Id == request.ChildId, cancellationToken);

            if (!childExists)
            {
                throw new NotFoundException("Child", request.ChildId);
            }

            var contact = new EmergencyContact
            {
                ChildId = request.ChildId,
                Name = request.Name,
                Relationship = request.Relationship,
                Phone = request.Phone
            };

            await _unitOfWork.Repository<EmergencyContact>().AddAsync(contact, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return contact.Id;
        }
    }
}
