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

        public ICollection<ChildPlanAssignment> ChildPlanAssignments { get; set; }
            = new List<ChildPlanAssignment>();
    }
}
