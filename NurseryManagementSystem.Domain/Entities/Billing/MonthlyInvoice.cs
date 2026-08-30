using NurseryManagementSystem.Domain.Common;
using NurseryManagementSystem.Domain.Entities.Children;
using NurseryManagementSystem.Domain.Entities.Identity;
using NurseryManagementSystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace NurseryManagementSystem.Domain.Entities.Billing
{
    public class MonthlyInvoice : AuditableEntity
    {
        public Guid ChildId { get; set; }

        public int BillingMonth { get; set; }

        public int BillingYear { get; set; }

        public decimal PlanFee { get; set; }

        public decimal TotalOvertimeFee { get; set; }

        public decimal GrandTotal { get; set; }
        public decimal AdjustmentAmount { get; set; }
        public string? AdjustmentReason { get; set; }
        public decimal PenaltyAmount { get; set; }
        public int LatePickupDays { get; set; }
        public decimal LatePickupFinePerDay { get; set; }
        public decimal OvertimeRate { get; set; }
        public decimal OvertimeHours { get; set; }
        public Guid? PlanId { get; set; }
        public string? PlanName { get; set; }
        public string Currency { get; set; } = "AED";
        public string? ParentFullName { get; set; }
        public string? ParentPhone { get; set; }

        public InvoiceStatus Status { get; set; } = InvoiceStatus.Pending;

        public DateTime? PaidAt { get; set; }

        public Guid? MarkedPaidById { get; set; }

        public Child Child { get; set; } = null!;

        public AppUser? MarkedPaidBy { get; set; }
    }
}
