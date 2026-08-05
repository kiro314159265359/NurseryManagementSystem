using NurseryManagementSystem.Domain.Common;
using NurseryManagementSystem.Domain.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace NurseryManagementSystem.Domain.Entities.Schedule
{
    public class DailyScheduleSlot : AuditableEntity
    {
        public TimeOnly StartTime { get; set; }

        public TimeOnly EndTime { get; set; }

        public string ActivityName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int SortOrder { get; set; }

        public bool IsActive { get; set; } = true;

        public Guid? LastModifiedById { get; set; }

        public AppUser? LastModifiedBy { get; set; }
    }
}
