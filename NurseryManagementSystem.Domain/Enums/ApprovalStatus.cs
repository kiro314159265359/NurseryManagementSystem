using System.Text.Json.Serialization;

namespace NurseryManagementSystem.Domain.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter<ApprovalStatus>))]
    public enum ApprovalStatus
    {
        Pending = 1,
        Approved = 2,
        Rejected = 3
    }
}
