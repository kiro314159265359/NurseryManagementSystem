using NurseryManagementSystem.Domain.Common;
using NurseryManagementSystem.Domain.Entities.Children;
using NurseryManagementSystem.Domain.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace NurseryManagementSystem.Domain.Entities.Plans
{
    public class ChildPlanAssignment : BaseEntity
    {
        public Guid ChildId { get; set; }

        public Guid PlanId { get; set; }

        public DateOnly StartDate { get; set; }

        public DateOnly? EndDate { get; set; }

        public Guid AssignedById { get; set; }

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
        public string PlanNameSnapshot { get; set; } = string.Empty;
        public string PlanCategorySnapshot { get; set; } = string.Empty;
        public decimal PriceSnapshot { get; set; }
        public int DurationHoursSnapshot { get; set; }
        public int DaysPerCycleSnapshot { get; set; }
        public string CurrencySnapshot { get; set; } = "AED";

        public Child Child { get; set; } = null!;

        public SubscriptionPlan Plan { get; set; } = null!;

        public AppUser AssignedBy { get; set; } = null!;
    }
}
