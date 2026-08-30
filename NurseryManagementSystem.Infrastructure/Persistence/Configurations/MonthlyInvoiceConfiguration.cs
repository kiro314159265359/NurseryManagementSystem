using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NurseryManagementSystem.Domain.Entities.Billing;

namespace NurseryManagementSystem.Infrastructure.Persistence.Configurations
{
    public class MonthlyInvoiceConfiguration : IEntityTypeConfiguration<MonthlyInvoice>
    {
        public void Configure(EntityTypeBuilder<MonthlyInvoice> builder)
        {
            builder.ToTable("MonthlyInvoices");

            builder.HasKey(i => i.Id);

            builder.Property(i => i.PlanFee).HasPrecision(18, 2);
            builder.Property(i => i.TotalOvertimeFee).HasPrecision(18, 2);
            builder.Property(i => i.GrandTotal).HasPrecision(18, 2);
            builder.Property(i => i.OvertimeHours).HasPrecision(18, 2);
            builder.Property(i => i.OvertimeRate).HasPrecision(18, 2);
            builder.Property(i => i.LatePickupFinePerDay).HasPrecision(18, 2);
            builder.Property(i => i.PenaltyAmount).HasPrecision(18, 2);
            builder.Property(i => i.AdjustmentAmount).HasPrecision(18, 2);
            builder.Property(i => i.PlanName).HasMaxLength(150);
            builder.Property(i => i.Currency).HasMaxLength(3);
            builder.Property(i => i.ParentFullName).HasMaxLength(200);
            builder.Property(i => i.ParentPhone).HasMaxLength(30);

            builder.Property(i => i.Status).HasConversion<int>();

            builder.HasIndex(i => new { i.ChildId, i.BillingYear, i.BillingMonth })
                .IsUnique();

            builder.HasOne(i => i.MarkedPaidBy)
                .WithMany()
                .HasForeignKey(i => i.MarkedPaidById)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
