using System.Text.Json.Serialization;

namespace HybridApp.ExternalProvider;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
	UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
	RespectNullableAnnotations = true, RespectRequiredConstructorParameters = true)]
[JsonSerializable(typeof(PluginResult))]
public partial class PluginJsonContext : JsonSerializerContext;
