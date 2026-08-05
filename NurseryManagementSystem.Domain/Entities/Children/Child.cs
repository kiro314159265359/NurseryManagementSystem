using NurseryManagementSystem.Domain.Common;
using System;
using System.Collections.Generic;
using NurseryManagementSystem.Domain.Entities.Attendance;
using System.Text;
using NurseryManagementSystem.Domain.Entities.Plans;
using NurseryManagementSystem.Domain.Entities.Billing;

namespace NurseryManagementSystem.Domain.Entities.Children
{
    public class Child : AuditableEntity
    {
        public string FullName { get; set; } = string.Empty;
        public DateOnly DateOfBirth { get; set; }
        public DateOnly EnrollmentDate { get; set; }

        public string Nationality { get; set; } = string.Empty;
        public string Religion { get; set; } = string.Empty;
        public string HomeAddress { get; set; } = string.Empty;
        public string? Allergies { get; set; }

        public string QrCode { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;

        public Mother Mother { get; set; } = null!;
        public Father Father { get; set; } = null!;
        public Agreement Agreement { get; set; } = null!;

        public ICollection<EmergencyContact> EmergencyContacts { get; set; } = new List<EmergencyContact>();
        public ICollection<ChildAttendance> Attendances { get; set; } = new List<ChildAttendance>();
        public ICollection<ChildPlanAssignment> PlanAssignments { get; set; } = new List<ChildPlanAssignment>();
        public ICollection<MonthlyInvoice> Invoices { get; set; } = new List<MonthlyInvoice>();
    }
}
