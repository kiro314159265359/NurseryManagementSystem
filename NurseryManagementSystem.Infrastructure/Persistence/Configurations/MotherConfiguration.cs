using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NurseryManagementSystem.Domain.Entities.Children;

namespace NurseryManagementSystem.Infrastructure.Persistence.Configurations
{
    public class MotherConfiguration : IEntityTypeConfiguration<Mother>
    {
        public void Configure(EntityTypeBuilder<Mother> builder)
        {
            builder.ToTable("Mothers");

            builder.HasKey(m => m.Id);

            builder.Property(m => m.FullName).HasMaxLength(200);
            builder.Property(m => m.Phone).HasMaxLength(30);
            builder.Property(m => m.Email).HasMaxLength(256);
            builder.Property(m => m.Occupation).HasMaxLength(150);
            builder.Property(m => m.JobTitle).HasMaxLength(150);
            builder.Property(m => m.CompanyName).HasMaxLength(200);
            builder.Property(m => m.WorkPhone).HasMaxLength(30);
            builder.Property(m => m.Address).HasMaxLength(500);
        }
    }
}
