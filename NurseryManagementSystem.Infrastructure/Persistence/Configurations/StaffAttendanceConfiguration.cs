using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NurseryManagementSystem.Domain.Entities.Attendance;

namespace NurseryManagementSystem.Infrastructure.Persistence.Configurations
{
    public class StaffAttendanceConfiguration : IEntityTypeConfiguration<StaffAttendance>
    {
        public void Configure(EntityTypeBuilder<StaffAttendance> builder)
        {
            builder.ToTable("StaffAttendances");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.ScanType).HasConversion<int>();

            builder.HasIndex(s => new { s.UserId, s.AttendanceDate });

            builder.HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
