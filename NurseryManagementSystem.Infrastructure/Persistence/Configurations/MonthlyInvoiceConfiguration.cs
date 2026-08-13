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
