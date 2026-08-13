using FluentValidation;
using MediatR;
using NurseryManagementSystem.Application.Common.Exceptions;
using NurseryManagementSystem.Application.Common.Interfaces;
using NurseryManagementSystem.Domain.Entities.Schedule;

namespace NurseryManagementSystem.Application.Features.Schedule.Commands
{
    public record UpdateScheduleSlotCommand(
        Guid Id,
        TimeOnly StartTime,
        TimeOnly EndTime,
        string ActivityName,
        string? Description,
        int SortOrder,
        bool IsActive) : IRequest<Unit>;

    public class UpdateScheduleSlotCommandValidator : AbstractValidator<UpdateScheduleSlotCommand>
    {
        public UpdateScheduleSlotCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.ActivityName).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Description).MaximumLength(1000);
            RuleFor(x => x.EndTime).GreaterThan(x => x.StartTime);
            RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
        }
    }

    public class UpdateScheduleSlotCommandHandler : IRequestHandler<UpdateScheduleSlotCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;

        public UpdateScheduleSlotCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        public async Task<Unit> Handle(UpdateScheduleSlotCommand request, CancellationToken cancellationToken)
        {
            var repository = _unitOfWork.Repository<DailyScheduleSlot>();

            var slot = await repository.GetByIdAsync(request.Id, cancellationToken);
            if (slot is null)
            {
                throw new NotFoundException("DailyScheduleSlot", request.Id);
            }

            slot.StartTime = request.StartTime;
            slot.EndTime = request.EndTime;
            slot.ActivityName = request.ActivityName;
            slot.Description = request.Description;
            slot.SortOrder = request.SortOrder;
            slot.IsActive = request.IsActive;
            slot.LastModifiedById = _currentUser.UserId;

            repository.Update(slot);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
