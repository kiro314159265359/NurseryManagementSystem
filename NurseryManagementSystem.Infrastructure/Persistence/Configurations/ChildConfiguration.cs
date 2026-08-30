using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NurseryManagementSystem.Domain.Entities.Children;
using NurseryManagementSystem.Domain.Enums;

namespace NurseryManagementSystem.Infrastructure.Persistence.Configurations
{
    public class ChildConfiguration : IEntityTypeConfiguration<Child>
    {
        public void Configure(EntityTypeBuilder<Child> builder)
        {
            builder.ToTable("Children");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.FullName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(c => c.Nationality).HasMaxLength(100);
            builder.Property(c => c.Religion).HasMaxLength(100);
            builder.Property(c => c.HomeAddress).HasMaxLength(500);
            builder.Property(c => c.Allergies).HasMaxLength(1000);

            builder.Property(c => c.QrCode)
                .IsRequired()
                .HasMaxLength(200);

            builder.HasIndex(c => c.QrCode).IsUnique();

            builder.Property(c => c.IsActive).HasDefaultValue(true);

            builder.Property(c => c.ApprovalStatus)
                .HasConversion<int>()
                .HasDefaultValue(ApprovalStatus.Approved);

            builder.Property(c => c.RejectionReason).HasMaxLength(500);

            builder.HasOne(c => c.ParentUser)
                .WithMany(u => u.Children)
                .HasForeignKey(c => c.ParentUserId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasIndex(c => new { c.ParentUserId, c.ApprovalStatus });

            builder.HasOne(c => c.Mother)
                .WithOne(m => m.Child)
                .HasForeignKey<Mother>(m => m.ChildId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(c => c.Father)
                .WithOne(f => f.Child)
                .HasForeignKey<Father>(f => f.ChildId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(c => c.Agreement)
                .WithOne(a => a.Child)
                .HasForeignKey<Agreement>(a => a.ChildId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(c => c.EmergencyContacts)
                .WithOne(e => e.Child)
                .HasForeignKey(e => e.ChildId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(c => c.Attendances)
                .WithOne(a => a.Child)
                .HasForeignKey(a => a.ChildId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(c => c.PlanAssignments)
                .WithOne(pa => pa.Child)
                .HasForeignKey(pa => pa.ChildId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(c => c.Invoices)
                .WithOne(i => i.Child)
                .HasForeignKey(i => i.ChildId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
