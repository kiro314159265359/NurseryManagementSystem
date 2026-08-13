using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NurseryManagementSystem.Domain.Entities.Children;

namespace NurseryManagementSystem.Infrastructure.Persistence.Configurations
{
    public class FatherConfiguration : IEntityTypeConfiguration<Father>
    {
        public void Configure(EntityTypeBuilder<Father> builder)
        {
            builder.ToTable("Fathers");

            builder.HasKey(f => f.Id);

            builder.Property(f => f.Phone).HasMaxLength(30);
            builder.Property(f => f.Email).HasMaxLength(256);
            builder.Property(f => f.Occupation).HasMaxLength(150);
            builder.Property(f => f.JobTitle).HasMaxLength(150);
            builder.Property(f => f.CompanyName).HasMaxLength(200);
            builder.Property(f => f.WorkPhone).HasMaxLength(30);
            builder.Property(f => f.Address).HasMaxLength(500);
        }
    }
}
