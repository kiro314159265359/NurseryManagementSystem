using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace NurseryManagementSystem.Domain.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter<ScanType>))]
    public enum ScanType
    {
        QRCode = 1,
        Barcode = 2,
        Manual = 3
    }
}
