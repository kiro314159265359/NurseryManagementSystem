using MediatR;
using NurseryManagementSystem.Application.Common.Exceptions;
using NurseryManagementSystem.Application.Common.Interfaces;
using NurseryManagementSystem.Domain.Entities.Schedule;

namespace NurseryManagementSystem.Application.Features.Schedule.Commands
{
    public record DeleteScheduleSlotCommand(Guid Id) : IRequest<Unit>;

    public class DeleteScheduleSlotCommandHandler : IRequestHandler<DeleteScheduleSlotCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeleteScheduleSlotCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Unit> Handle(DeleteScheduleSlotCommand request, CancellationToken cancellationToken)
        {
            var repository = _unitOfWork.Repository<DailyScheduleSlot>();

            var slot = await repository.GetByIdAsync(request.Id, cancellationToken);
            if (slot is null)
            {
                throw new NotFoundException("DailyScheduleSlot", request.Id);
            }

            repository.Remove(slot);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
