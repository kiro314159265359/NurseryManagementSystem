using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NurseryManagementSystem.Domain.Entities.Attendance;
using NurseryManagementSystem.Domain.Entities.Billing;
using NurseryManagementSystem.Domain.Entities.Children;
using NurseryManagementSystem.Domain.Entities.Identity;
using NurseryManagementSystem.Domain.Entities.Plans;
using NurseryManagementSystem.Domain.Entities.Schedule;

namespace NurseryManagementSystem.Infrastructure.Persistence
{
    public class AppDbContext : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Child> Children => Set<Child>();
        public DbSet<Mother> Mothers => Set<Mother>();
        public DbSet<Father> Fathers => Set<Father>();
        public DbSet<Agreement> Agreements => Set<Agreement>();
        public DbSet<EmergencyContact> EmergencyContacts => Set<EmergencyContact>();
        public DbSet<ParentChild> ParentChildren => Set<ParentChild>();

        public DbSet<ChildAttendance> ChildAttendances => Set<ChildAttendance>();
        public DbSet<StaffAttendance> StaffAttendances => Set<StaffAttendance>();
        public DbSet<SessionLog> SessionLogs => Set<SessionLog>();

        public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
        public DbSet<ChildPlanAssignment> ChildPlanAssignments => Set<ChildPlanAssignment>();

        public DbSet<MonthlyInvoice> MonthlyInvoices => Set<MonthlyInvoice>();

        public DbSet<DailyScheduleSlot> DailyScheduleSlots => Set<DailyScheduleSlot>();

        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}
