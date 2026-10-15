using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Maui.Devices.Sensors;

namespace Microsoft.Maui.Controls.HybridWebApps.Essentials;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
	AllowDuplicateProperties = false,
	Converters = [typeof(NativeAltitudeReferenceSystemConverter)],
	UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
	RespectNullableAnnotations = true, RespectRequiredConstructorParameters = true)]
[JsonSerializable(typeof(Location))]
internal partial class NativeLocationJsonContext : JsonSerializerContext;

internal sealed class NativeAltitudeReferenceSystemConverter() : JsonStringEnumConverter<AltitudeReferenceSystem>(
	JsonNamingPolicy.CamelCase, allowIntegerValues: false);
