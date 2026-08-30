using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NurseryManagementSystem.Domain.Entities.Children;

namespace NurseryManagementSystem.Infrastructure.Persistence.Configurations;

public class ParentChildConfiguration : IEntityTypeConfiguration<ParentChild>
{
    public void Configure(EntityTypeBuilder<ParentChild> builder)
    {
        builder.ToTable("ParentChildren");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Relationship).IsRequired().HasMaxLength(50);
        builder.HasIndex(x => new { x.ParentUserId, x.ChildId }).IsUnique();

        builder.HasOne(x => x.ParentUser)
            .WithMany(x => x.ParentChildren)
            .HasForeignKey(x => x.ParentUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Child)
            .WithMany(x => x.ParentLinks)
            .HasForeignKey(x => x.ChildId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
