using Microsoft.AspNetCore.Identity;
using NurseryManagementSystem.Domain.Enums;
using NurseryManagementSystem.Domain.Entities.Children;
using System;
using System.Collections.Generic;
using System.Text;

namespace NurseryManagementSystem.Domain.Entities.Identity
{
    public class AppUser : IdentityUser<Guid>
    {
        public string FullName { get; set; } = string.Empty;

        public UserRole Role { get; set; }

        public string? QrCode { get; set; }

        public bool IsActive { get; set; } = true;

        public ApprovalStatus ApprovalStatus { get; set; } = ApprovalStatus.Approved;

        public ParentRelationship? ParentRelationship { get; set; }

        public ICollection<Child> Children { get; set; }
            = new List<Child>();

        public ICollection<RefreshToken> RefreshTokens { get; set; }
            = new List<RefreshToken>();

        public ICollection<ParentChild> ParentChildren { get; set; }
            = new List<ParentChild>();
    }
}
