using NurseryManagementSystem.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace NurseryManagementSystem.Domain.Entities.Plans
{
    public class SubscriptionPlan : AuditableEntity
    {
        public string Name { get; set; } = string.Empty;

        public int DurationHours { get; set; }

        public bool IsWeekend { get; set; }

        public decimal MonthlyFee { get; set; }

        public decimal DailyOvertimeFee { get; set; }

        public string Category { get; set; } = "Monthly Packages";
        public string BillingCycle { get; set; } = "Monthly";
        public int DaysPerCycle { get; set; } = 5;
        public bool IsFullDay { get; set; }
        public string? BadgeText { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsActive { get; set; } = true;
        public string Currency { get; set; } = "AED";
        public int DisplayOrder { get; set; }

        public ICollection<ChildPlanAssignment> ChildPlanAssignments { get; set; }
            = new List<ChildPlanAssignment>();
    }
}
