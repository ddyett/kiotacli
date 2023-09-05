// <auto-generated/>
using System.Runtime.Serialization;
using System;
namespace GetUserClient.ApiClient.Models {
    public enum SimulationStatus {
        [EnumMember(Value = "unknown")]
        Unknown,
        [EnumMember(Value = "draft")]
        Draft,
        [EnumMember(Value = "running")]
        Running,
        [EnumMember(Value = "scheduled")]
        Scheduled,
        [EnumMember(Value = "succeeded")]
        Succeeded,
        [EnumMember(Value = "failed")]
        Failed,
        [EnumMember(Value = "cancelled")]
        Cancelled,
        [EnumMember(Value = "excluded")]
        Excluded,
        [EnumMember(Value = "unknownFutureValue")]
        UnknownFutureValue,
    }
}
