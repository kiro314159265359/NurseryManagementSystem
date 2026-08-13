using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NurseryManagementSystem.Domain.Entities.Attendance;

namespace NurseryManagementSystem.Infrastructure.Persistence.Configurations
{
    public class SessionLogConfiguration : IEntityTypeConfiguration<SessionLog>
    {
        public void Configure(EntityTypeBuilder<SessionLog> builder)
        {
            builder.ToTable("SessionLogs");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.IpAddress).HasMaxLength(45);

            builder.HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
