using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace NurseryManagementSystem.Domain.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter<InvoiceStatus>))]
    public enum InvoiceStatus
    {
        Pending = 1,
        Paid = 2,
        Overdue = 3,
        Cancelled = 4
    }
}
