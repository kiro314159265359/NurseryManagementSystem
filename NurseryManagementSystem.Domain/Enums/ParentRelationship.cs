using System.Text.Json.Serialization;

namespace NurseryManagementSystem.Domain.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter<ParentRelationship>))]
    public enum ParentRelationship
    {
        Mother = 1,
        Father = 2
    }
}
