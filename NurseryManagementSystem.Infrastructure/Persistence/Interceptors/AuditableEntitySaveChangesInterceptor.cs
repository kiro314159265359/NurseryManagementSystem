using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NurseryManagementSystem.Application.Common.Interfaces;
using NurseryManagementSystem.Domain.Common;
using NurseryManagementSystem.Domain.Entities.Audit;

namespace NurseryManagementSystem.Infrastructure.Persistence.Interceptors
{
    public class AuditableEntitySaveChangesInterceptor : SaveChangesInterceptor
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IDateTimeProvider _dateTimeProvider;

        public AuditableEntitySaveChangesInterceptor(
            ICurrentUserService currentUserService,
            IDateTimeProvider dateTimeProvider)
        {
            _currentUserService = currentUserService;
            _dateTimeProvider = dateTimeProvider;
        }

        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData,
            InterceptionResult<int> result)
        {
            ApplyAudit(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            ApplyAudit(eventData.Context);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private void ApplyAudit(DbContext? context)
        {
            if (context is null)
            {
                return;
            }

            var utcNow = _dateTimeProvider.UtcNow;
            var userId = _currentUserService.UserId;

            var auditEntries = context.ChangeTracker.Entries<BaseEntity>()
                .Where(entry => entry.Entity is not AuditLogEntry
                    && entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
                .Select(entry => new AuditLogEntry
                {
                    Id = Guid.NewGuid(),
                    At = utcNow,
                    ActorUserId = userId,
                    ActorName = _currentUserService.UserName ?? "system",
                    Action = $"{entry.Metadata.ClrType.Name}{entry.State}",
                    SubjectType = entry.Metadata.ClrType.Name,
                    SubjectId = entry.Entity.Id,
                    SubjectName = entry.Properties.FirstOrDefault(p => p.Metadata.Name is "FullName" or "Name")?.CurrentValue?.ToString(),
                    Details = entry.State.ToString()
                })
                .ToList();

            if (auditEntries.Count > 0)
            {
                context.Set<AuditLogEntry>().AddRange(auditEntries);
            }

            foreach (var entry in context.ChangeTracker.Entries<AuditableEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = utcNow;
                        entry.Entity.CreatedBy = userId;
                        break;

                    case EntityState.Modified:
                        entry.Entity.UpdatedAt = utcNow;
                        entry.Entity.UpdatedBy = userId;
                        break;
                }
            }
        }
    }
}
