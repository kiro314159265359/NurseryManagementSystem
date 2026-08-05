using NurseryManagementSystem.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace NurseryManagementSystem.Domain.Entities.Children
{
    public class EmergencyContact : BaseEntity
    {
        public Guid ChildId { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Relationship { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;

        public Child Child { get; set; } = null!;
    }
}
