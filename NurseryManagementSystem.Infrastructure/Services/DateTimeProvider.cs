using NurseryManagementSystem.Application.Common.Interfaces;

namespace NurseryManagementSystem.Infrastructure.Services
{
    public class DateTimeProvider : IDateTimeProvider
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}
