using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace NurseryManagementSystem.Domain.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter<UserRole>))]
    public enum UserRole
    {
        SuperAdmin = 1,
        SubAdmin = 2,
        Parent = 3
    }
}
