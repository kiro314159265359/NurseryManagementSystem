using NurseryManagementSystem.Domain.Common;
using NurseryManagementSystem.Domain.Entities.Identity;
using NurseryManagementSystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace NurseryManagementSystem.Domain.Entities.Attendance
{
    public class StaffAttendance : BaseEntity
    {
        public Guid UserId { get; set; }

        public DateTime ClockIn { get; set; }
        public DateTime? ClockOut { get; set; }
        public DateOnly AttendanceDate { get; set; }

        public ScanType ScanType { get; set; }

        public AppUser User { get; set; } = null!;
    }
}
