using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NurseryManagementSystem.Domain.Entities.Schedule;

namespace NurseryManagementSystem.Infrastructure.Persistence.Configurations
{
    public class DailyScheduleSlotConfiguration : IEntityTypeConfiguration<DailyScheduleSlot>
    {
        public void Configure(EntityTypeBuilder<DailyScheduleSlot> builder)
        {
            builder.ToTable("DailyScheduleSlots");

            builder.HasKey(d => d.Id);

            builder.Property(d => d.ActivityName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(d => d.Description).HasMaxLength(1000);

            builder.Property(d => d.IsActive).HasDefaultValue(true);

            builder.HasOne(d => d.LastModifiedBy)
                .WithMany()
                .HasForeignKey(d => d.LastModifiedById)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
