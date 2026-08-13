using MediatR;
using NurseryManagementSystem.Application.Common.Interfaces;
using NurseryManagementSystem.Domain.Entities.Attendance;

namespace NurseryManagementSystem.Application.Features.SessionLogs.Commands
{
    public record CreateSessionLogCommand(Guid UserId, string IpAddress) : IRequest<Guid>;

    public class CreateSessionLogCommandHandler : IRequestHandler<CreateSessionLogCommand, Guid>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateTimeProvider _dateTime;

        public CreateSessionLogCommandHandler(IUnitOfWork unitOfWork, IDateTimeProvider dateTime)
        {
            _unitOfWork = unitOfWork;
            _dateTime = dateTime;
        }

        public async Task<Guid> Handle(CreateSessionLogCommand request, CancellationToken cancellationToken)
        {
            var sessionLog = new SessionLog
            {
                UserId = request.UserId,
                LoginAt = _dateTime.UtcNow,
                IpAddress = request.IpAddress
            };

            await _unitOfWork.Repository<SessionLog>().AddAsync(sessionLog, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return sessionLog.Id;
        }
    }
}
