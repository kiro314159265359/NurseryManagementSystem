using NurseryManagementSystem.Domain.Common;
using NurseryManagementSystem.Domain.Enums;
using NurseryManagementSystem.Domain.Entities.Children;
using System;
using System.Collections.Generic;
using System.Text;

namespace NurseryManagementSystem.Domain.Entities.Attendance
{
    public class ChildAttendance : BaseEntity
    {
        public Guid ChildId { get; set; }

        public DateTime ClockIn { get; set; }
        public DateTime? ClockOut { get; set; }
        public DateOnly AttendanceDate { get; set; }

        public decimal HoursStayed { get; set; }
        public decimal OvertimeHours { get; set; }
        public decimal OvertimeFee { get; set; }

        public ScanType ScanType { get; set; }

        public Child Child { get; set; } = null!;
    }
}
