using NurseryManagementSystem.Domain.Common;
using NurseryManagementSystem.Domain.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace NurseryManagementSystem.Domain.Entities.Attendance
{
    public class SessionLog : BaseEntity
    {
        public Guid UserId { get; set; }

        public DateTime LoginAt { get; set; }
        public DateTime? LogoutAt { get; set; }

        public string IpAddress { get; set; } = string.Empty;

        public AppUser User { get; set; } = null!;
    }
}
