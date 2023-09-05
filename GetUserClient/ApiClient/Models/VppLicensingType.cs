// <auto-generated/>
using Microsoft.Kiota.Abstractions.Serialization;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
namespace GetUserClient.ApiClient.Models {
    /// <summary>
    /// Contains properties for iOS Volume-Purchased Program (Vpp) Licensing Type.
    /// </summary>
    public class VppLicensingType : IParsable {
        /// <summary>The OdataType property</summary>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
#nullable enable
        public string? OdataType { get; set; }
#nullable restore
#else
        public string OdataType { get; set; }
#endif
        /// <summary>Whether the program supports the device licensing type.</summary>
        public bool? SupportsDeviceLicensing { get; set; }
        /// <summary>Whether the program supports the user licensing type.</summary>
        public bool? SupportsUserLicensing { get; set; }
        /// <summary>
        /// Creates a new instance of the appropriate class based on discriminator value
        /// </summary>
        /// <param name="parseNode">The parse node to use to read the discriminator value and create the object</param>
        public static VppLicensingType CreateFromDiscriminatorValue(IParseNode parseNode) {
            _ = parseNode ?? throw new ArgumentNullException(nameof(parseNode));
            return new VppLicensingType();
        }
        /// <summary>
        /// The deserialization information for the current model
        /// </summary>
        public IDictionary<string, Action<IParseNode>> GetFieldDeserializers() {
            return new Dictionary<string, Action<IParseNode>> {
                {"@odata.type", n => { OdataType = n.GetStringValue(); } },
                {"supportsDeviceLicensing", n => { SupportsDeviceLicensing = n.GetBoolValue(); } },
                {"supportsUserLicensing", n => { SupportsUserLicensing = n.GetBoolValue(); } },
            };
        }
        /// <summary>
        /// Serializes information the current object
        /// </summary>
        /// <param name="writer">Serialization writer to use to serialize this model</param>
        public void Serialize(ISerializationWriter writer) {
            _ = writer ?? throw new ArgumentNullException(nameof(writer));
            writer.WriteStringValue("@odata.type", OdataType);
            writer.WriteBoolValue("supportsDeviceLicensing", SupportsDeviceLicensing);
            writer.WriteBoolValue("supportsUserLicensing", SupportsUserLicensing);
        }
    }
}