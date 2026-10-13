using System.Text.Json.Serialization;

namespace Maui.HybridApp;

/// <summary>Generated metadata only for protocol/control contracts and the narrow string target.</summary>
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
	UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
	RespectNullableAnnotations = true, RespectRequiredConstructorParameters = true)]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(Request))]
[JsonSerializable(typeof(Response))]
[JsonSerializable(typeof(NativeEvent))]
[JsonSerializable(typeof(DocumentMessage))]
[JsonSerializable(typeof(EmptyArgs))]
[JsonSerializable(typeof(ReadyResult))]
[JsonSerializable(typeof(Ack))]
[JsonSerializable(typeof(TopicArgs))]
[JsonSerializable(typeof(CancelArgs))]
public partial class ProtocolJsonContext : JsonSerializerContext;
