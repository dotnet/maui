using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Microsoft.Maui.Controls.HybridWebApps;

/// <summary>Generated metadata only for protocol/control contracts and the narrow string target.</summary>
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
	UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
	RespectNullableAnnotations = true, RespectRequiredConstructorParameters = true)]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(Request))]
[JsonSerializable(typeof(Response))]
[JsonSerializable(typeof(NativeEvent))]
[JsonSerializable(typeof(DocumentMessage))]
[JsonSerializable(typeof(ReadyResult))]
[JsonSerializable(typeof(Ack))]
[JsonSerializable(typeof(CommandError))]
[JsonSerializable(typeof(ChannelMessage))]
internal partial class ProtocolSerializerContext : JsonSerializerContext;

/// <summary>Nullable-annotated protocol metadata and the narrow string invocation context.</summary>
public sealed class ProtocolJsonContext : JsonSerializerContext
{
	readonly ProtocolSerializerContext _source;
	/// <summary>Creates the strict protocol context.</summary>
	public ProtocolJsonContext() : this(HybridJsonDefaults.CreateOptions()) { }
	/// <summary>Creates protocol metadata with explicit serializer options.</summary>
	/// <param name="options">The protocol serializer options.</param>
	public ProtocolJsonContext(JsonSerializerOptions options) : base(options) => _source = new(new JsonSerializerOptions(options));
	/// <summary>Gets the shared strict protocol context.</summary>
	public static ProtocolJsonContext Default { get; } = new();
	/// <inheritdoc/>
	protected override JsonSerializerOptions GeneratedSerializerOptions => HybridJsonDefaults.CreateOptions();
	/// <inheritdoc/>
	public override JsonTypeInfo? GetTypeInfo(Type type) => _source.GetTypeInfo(type);
	/// <summary>Gets request metadata.</summary>
	public JsonTypeInfo<Request> Request => _source.Request;
	/// <summary>Gets response metadata.</summary>
	public JsonTypeInfo<Response> Response => _source.Response;
	/// <summary>Gets native-event metadata.</summary>
	public JsonTypeInfo<NativeEvent> NativeEvent => _source.NativeEvent;
	/// <summary>Gets document lifecycle metadata.</summary>
	public JsonTypeInfo<DocumentMessage> DocumentMessage => _source.DocumentMessage;
	/// <summary>Gets connection capability metadata.</summary>
	public JsonTypeInfo<ReadyResult> ReadyResult => _source.ReadyResult;
	/// <summary>Gets acknowledgement metadata.</summary>
	public JsonTypeInfo<Ack> Ack => _source.Ack;
	/// <summary>Gets bounded error metadata.</summary>
	public JsonTypeInfo<CommandError> CommandError => _source.CommandError;
	/// <summary>Gets binary-channel control metadata.</summary>
	public JsonTypeInfo<ChannelMessage> ChannelMessage => _source.ChannelMessage;
	/// <summary>Gets narrow bridge string metadata.</summary>
	public JsonTypeInfo<string> String => _source.String;
	/// <summary>Gets detached JSON metadata.</summary>
	public JsonTypeInfo<JsonElement> JsonElement => _source.JsonElement;
}
