using NurseryManagementSystem.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace NurseryManagementSystem.Domain.Entities.Children
{
    public class Agreement : BaseEntity
    {
        public Guid ChildId { get; set; }

        public bool MediaPermission { get; set; }
        public string ParentSignature { get; set; } = string.Empty;
        public DateOnly SignedDate { get; set; }
        public bool AcceptedTerms { get; set; }

        public Child Child { get; set; } = null!;
    }
}
