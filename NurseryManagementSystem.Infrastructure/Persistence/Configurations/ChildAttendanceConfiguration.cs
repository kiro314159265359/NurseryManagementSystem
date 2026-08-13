using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NurseryManagementSystem.Domain.Entities.Attendance;

namespace NurseryManagementSystem.Infrastructure.Persistence.Configurations
{
    public class ChildAttendanceConfiguration : IEntityTypeConfiguration<ChildAttendance>
    {
        public void Configure(EntityTypeBuilder<ChildAttendance> builder)
        {
            builder.ToTable("ChildAttendances");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.HoursStayed).HasPrecision(6, 2);
            builder.Property(a => a.OvertimeHours).HasPrecision(6, 2);
            builder.Property(a => a.OvertimeFee).HasPrecision(18, 2);

            builder.Property(a => a.ScanType).HasConversion<int>();

            builder.HasIndex(a => new { a.ChildId, a.AttendanceDate });
        }
    }
}
