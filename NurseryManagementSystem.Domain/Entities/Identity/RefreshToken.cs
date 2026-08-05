using NurseryManagementSystem.Domain.Common;

namespace NurseryManagementSystem.Domain.Entities.Identity
{
    public class RefreshToken : BaseEntity
    {
        public string Token { get; set; } = string.Empty;

        public DateTime ExpiresAt { get; set; }

        public bool IsRevoked { get; set; }

        public Guid UserId { get; set; }

        public AppUser User { get; set; } = null!;
    }
}