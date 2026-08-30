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

        public ICollection<RefreshToken> RefreshTokens { get; set; }
            = new List<RefreshToken>();

        public ICollection<ParentChild> Children { get; set; }
            = new List<ParentChild>();
    }
}
