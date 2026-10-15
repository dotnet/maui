using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Microsoft.Maui.Controls.HybridWebApps;

/// <summary>Compiled strict JSON plumbing around generated typed readers and writers.</summary>
/// <typeparam name="T">The statically known contract.</typeparam>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class HybridJsonCodec<T>
{
	readonly Func<JsonElement, bool, T> _read;
	readonly Action<Utf8JsonWriter, T, bool> _write;

	/// <summary>Creates a primitive codec with compiled null checks.</summary>
	/// <param name="read">Reads a non-null value.</param><param name="write">Writes a non-null value.</param>
	/// <param name="supportsNull">Whether null has a default representation for this type.</param>
	public HybridJsonCodec(Func<JsonElement, T> read, Action<Utf8JsonWriter, T> write, bool supportsNull = false)
	{
		_read = (json, nullable) => supportsNull && json.ValueKind == JsonValueKind.Null
			? nullable ? default! : throw new JsonException("Null is not allowed.") : read(json);
		_write = (writer, value, nullable) =>
		{
			if (value is null)
			{
				if (!nullable)
					throw new JsonException("Null is not allowed.");
				writer.WriteNullValue();
			}
			else
				write(writer, value);
		};
	}

	/// <summary>Creates a generated contract codec with explicit use-site null handling.</summary>
	/// <param name="read">The typed generated reader.</param><param name="write">The typed generated writer.</param>
	public HybridJsonCodec(Func<JsonElement, bool, T> read, Action<Utf8JsonWriter, T, bool> write)
		=> (_read, _write) = (read, write);

	/// <summary>Reads one contract, enforcing its use-site nullability.</summary>
	/// <param name="json">The input value.</param><param name="nullable">Whether this use permits null.</param>
	/// <returns>The typed value.</returns>
	public T Read(JsonElement json, bool nullable)
		=> _read(json, nullable);

	/// <summary>Writes one contract, enforcing its use-site nullability.</summary>
	/// <param name="writer">The output writer.</param><param name="value">The typed value.</param>
	/// <param name="nullable">Whether this use permits null.</param>
	public void Write(Utf8JsonWriter writer, T value, bool nullable)
		=> _write(writer, value, nullable);

	/// <summary>Creates explicit value metadata owned by the consuming assembly's source context.</summary>
	/// <param name="context">The generated context.</param><returns>Typed source-originated metadata.</returns>
	public JsonTypeInfo<T> CreateInfo(JsonSerializerContext context)
	{
		var info = JsonMetadataServices.CreateValueInfo<T>(context.Options, new Converter(this));
		info.OriginatingResolver = context;
		return info;
	}

	sealed class Converter(HybridJsonCodec<T> codec) : JsonConverter<T>
	{
		public override bool HandleNull => true;
		public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			using var document = JsonDocument.ParseValue(ref reader);
			return codec.Read(document.RootElement, true);
		}
		public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options) =>
			codec.Write(writer, value, true);
	}
}

/// <summary>Strict options and null metadata shared by all generated providers.</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class HybridJsonDefaults
{
	/// <summary>Creates the strict options required by generated contracts.</summary>
	/// <returns>Fresh options for the consuming source context.</returns>
	public static JsonSerializerOptions CreateOptions() => new()
	{
		UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
		RespectNullableAnnotations = true,
		RespectRequiredConstructorParameters = true,
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		AllowDuplicateProperties = false,
	};

	/// <summary>Gets the explicit void/event-empty payload codec, never a general object serializer.</summary>
	public static HybridJsonCodec<object?> Null { get; } = new(
		json => json.ValueKind == JsonValueKind.Null ? null : throw new JsonException(),
		(_, _) => throw new JsonException("Only null is supported."), supportsNull: true);
}
