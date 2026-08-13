using FluentValidation;
using MediatR;
using NurseryManagementSystem.Application.Common.Interfaces;
using NurseryManagementSystem.Domain.Entities.Schedule;

namespace NurseryManagementSystem.Application.Features.Schedule.Commands
{
    public record CreateScheduleSlotCommand(
        TimeOnly StartTime,
        TimeOnly EndTime,
        string ActivityName,
        string? Description,
        int SortOrder) : IRequest<Guid>;

    public class CreateScheduleSlotCommandValidator : AbstractValidator<CreateScheduleSlotCommand>
    {
        public CreateScheduleSlotCommandValidator()
        {
            RuleFor(x => x.ActivityName).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Description).MaximumLength(1000);
            RuleFor(x => x.EndTime).GreaterThan(x => x.StartTime);
            RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
        }
    }

    public class CreateScheduleSlotCommandHandler : IRequestHandler<CreateScheduleSlotCommand, Guid>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;

        public CreateScheduleSlotCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        public async Task<Guid> Handle(CreateScheduleSlotCommand request, CancellationToken cancellationToken)
        {
            var slot = new DailyScheduleSlot
            {
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                ActivityName = request.ActivityName,
                Description = request.Description,
                SortOrder = request.SortOrder,
                IsActive = true,
                LastModifiedById = _currentUser.UserId
            };

            await _unitOfWork.Repository<DailyScheduleSlot>().AddAsync(slot, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return slot.Id;
        }
    }
}
