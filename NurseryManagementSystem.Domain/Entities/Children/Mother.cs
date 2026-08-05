using NurseryManagementSystem.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace NurseryManagementSystem.Domain.Entities.Children
{
    public class Mother : BaseEntity
    {
        public Guid ChildId { get; set; }

        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Occupation { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string WorkPhone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;

        public Child Child { get; set; } = null!;
    }
}
