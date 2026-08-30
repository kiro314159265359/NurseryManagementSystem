using NurseryManagementSystem.Domain.Common;
using NurseryManagementSystem.Domain.Entities.Identity;

namespace NurseryManagementSystem.Domain.Entities.Children;

public class ParentChild : BaseEntity
{
    public Guid ParentUserId { get; set; }
    public Guid ChildId { get; set; }
    public string Relationship { get; set; } = string.Empty;

    public AppUser ParentUser { get; set; } = null!;
    public Child Child { get; set; } = null!;
}
