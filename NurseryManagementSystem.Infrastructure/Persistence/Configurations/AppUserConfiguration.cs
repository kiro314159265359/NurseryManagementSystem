using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NurseryManagementSystem.Domain.Entities.Identity;
using NurseryManagementSystem.Domain.Enums;

namespace NurseryManagementSystem.Infrastructure.Persistence.Configurations
{
    public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
    {
        public void Configure(EntityTypeBuilder<AppUser> builder)
        {
            builder.Property(u => u.FullName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(u => u.Role).HasConversion<int>();

            builder.Property(u => u.QrCode).HasMaxLength(200);

            builder.Property(u => u.IsActive).HasDefaultValue(true);

            builder.Property(u => u.ApprovalStatus)
                .HasConversion<int>()
                .HasDefaultValue(ApprovalStatus.Approved);

            builder.Property(u => u.ParentRelationship).HasConversion<int?>();

            builder.HasIndex(u => u.QrCode).IsUnique();
        }
    }
}
