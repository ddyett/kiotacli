// <auto-generated/>
using Microsoft.Kiota.Abstractions.Serialization;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
namespace GetUserClient.ApiClient.Models.CallRecords {
    public class Endpoint : IParsable {
        /// <summary>The OdataType property</summary>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
#nullable enable
        public string? OdataType { get; set; }
#nullable restore
#else
        public string OdataType { get; set; }
#endif
        /// <summary>User-agent reported by this endpoint.</summary>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
#nullable enable
        public GetUserClient.ApiClient.Models.CallRecords.UserAgent? UserAgent { get; set; }
#nullable restore
#else
        public GetUserClient.ApiClient.Models.CallRecords.UserAgent UserAgent { get; set; }
#endif
        /// <summary>
        /// Creates a new instance of the appropriate class based on discriminator value
        /// </summary>
        /// <param name="parseNode">The parse node to use to read the discriminator value and create the object</param>
        public static Endpoint CreateFromDiscriminatorValue(IParseNode parseNode) {
            _ = parseNode ?? throw new ArgumentNullException(nameof(parseNode));
            var mappingValue = parseNode.GetChildNode("@odata.type")?.GetStringValue();
            return mappingValue switch {
                "#microsoft.graph.callRecords.participantEndpoint" => new ParticipantEndpoint(),
                "#microsoft.graph.callRecords.serviceEndpoint" => new ServiceEndpoint(),
                _ => new Endpoint(),
            };
        }
        /// <summary>
        /// The deserialization information for the current model
        /// </summary>
        public IDictionary<string, Action<IParseNode>> GetFieldDeserializers() {
            return new Dictionary<string, Action<IParseNode>> {
                {"@odata.type", n => { OdataType = n.GetStringValue(); } },
                {"userAgent", n => { UserAgent = n.GetObjectValue<GetUserClient.ApiClient.Models.CallRecords.UserAgent>(GetUserClient.ApiClient.Models.CallRecords.UserAgent.CreateFromDiscriminatorValue); } },
            };
        }
        /// <summary>
        /// Serializes information the current object
        /// </summary>
        /// <param name="writer">Serialization writer to use to serialize this model</param>
        public void Serialize(ISerializationWriter writer) {
            _ = writer ?? throw new ArgumentNullException(nameof(writer));
            writer.WriteStringValue("@odata.type", OdataType);
            writer.WriteObjectValue<GetUserClient.ApiClient.Models.CallRecords.UserAgent>("userAgent", UserAgent);
        }
    }
}
