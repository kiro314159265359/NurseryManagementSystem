using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NurseryManagementSystem.Domain.Entities.Plans;

namespace NurseryManagementSystem.Infrastructure.Persistence.Configurations
{
    public class ChildPlanAssignmentConfiguration : IEntityTypeConfiguration<ChildPlanAssignment>
    {
        public void Configure(EntityTypeBuilder<ChildPlanAssignment> builder)
        {
            builder.ToTable("ChildPlanAssignments");

            builder.HasKey(pa => pa.Id);

            builder.HasIndex(pa => new { pa.ChildId, pa.StartDate });

            builder.HasOne(pa => pa.Plan)
                .WithMany(p => p.ChildPlanAssignments)
                .HasForeignKey(pa => pa.PlanId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(pa => pa.AssignedBy)
                .WithMany()
                .HasForeignKey(pa => pa.AssignedById)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
