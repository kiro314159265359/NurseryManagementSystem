using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NurseryManagementSystem.Domain.Entities.Identity;

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

            builder.HasIndex(u => u.QrCode).IsUnique();
        }
    }
}
