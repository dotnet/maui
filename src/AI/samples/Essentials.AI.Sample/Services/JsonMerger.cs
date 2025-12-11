using System.Buffers;
using System.Text;
using System.Text.Json;

namespace Maui.Controls.Sample.Services;

/// <summary>
/// Merges JSON overlay documents onto a base document.
/// Useful for streaming translation scenarios where partial translations need to be merged
/// with the original content progressively.
/// </summary>
/// <remarks>
/// The merger performs a deep merge:
/// - For objects: properties from overlay replace matching properties in base
/// - For arrays: elements are merged by index position
/// - For primitives: overlay value replaces base value
/// - Missing overlay values preserve the base value
/// 
/// Create with a base JSON, then call <see cref="MergeOverlay"/> repeatedly
/// with incoming partial JSON to get progressive merged results.
/// </remarks>
internal sealed class JsonMerger
{
	private readonly ArrayBufferWriter<byte> _outputBuffer = new(initialCapacity: 4096);
	private readonly JsonDocument _baseDocument;

	/// <summary>
	/// Creates a new JsonMerger with the specified base JSON document.
	/// </summary>
	/// <param name="baseJson">The base JSON document (e.g., original English content).</param>
	public JsonMerger(string baseJson)
	{
		ArgumentException.ThrowIfNullOrEmpty(baseJson);

		_baseDocument = JsonDocument.Parse(baseJson);
	}

	/// <summary>
	/// Creates a new JsonMerger with the specified base JSON document as UTF-8 bytes.
	/// </summary>
	/// <param name="baseJsonUtf8">The base JSON document as UTF-8 bytes.</param>
	public JsonMerger(ReadOnlyMemory<byte> baseJsonUtf8)
	{
		if (baseJsonUtf8.IsEmpty)
			throw new ArgumentException("Base JSON cannot be empty.", nameof(baseJsonUtf8));
		
		_baseDocument = JsonDocument.Parse(baseJsonUtf8);
	}

	/// <summary>
	/// Gets the result of the last merge operation as UTF-8 bytes (zero-allocation view).
	/// </summary>
	public ReadOnlyMemory<byte> PartialJsonMemory => _outputBuffer.WrittenMemory;

	/// <summary>
	/// Gets the result of the last merge operation as UTF-8 bytes.
	/// </summary>
	public ReadOnlySpan<byte> ResultUtf8 => _outputBuffer.WrittenSpan;

	/// <summary>
	/// Gets the result of the last merge operation as a string.
	/// </summary>
	public string Result => Encoding.UTF8.GetString(_outputBuffer.WrittenSpan);

	/// <summary>
	/// Merges an overlay JSON string onto the base document.
	/// Can be called repeatedly with progressive partial JSON.
	/// </summary>
	/// <param name="overlayJson">The overlay JSON document (e.g., partial translation).</param>
	/// <returns>The merged JSON as a string.</returns>
	public string MergeOverlay(string overlayJson)
	{
		if (string.IsNullOrEmpty(overlayJson))
		{
			// No overlay - just serialize the base document
			MergeOverlayUtf8(ReadOnlySpan<byte>.Empty);
			return Result;
		}

		var overlayBytes = Encoding.UTF8.GetBytes(overlayJson);
		MergeOverlayUtf8(overlayBytes);
		return Result;
	}

	/// <summary>
	/// Merges an overlay JSON document onto the base document (UTF-8 bytes).
	/// </summary>
	/// <param name="overlayJson">The overlay JSON document as UTF-8 bytes.</param>
	public void MergeOverlayUtf8(ReadOnlySpan<byte> overlayJson)
	{
		_outputBuffer.Clear();

		if (overlayJson.IsEmpty)
		{
			// No overlay - just write the base document
			using var writer = new Utf8JsonWriter(_outputBuffer, new JsonWriterOptions { Indented = false });
			WriteElement(writer, _baseDocument.RootElement);
			writer.Flush();
			return;
		}

		// Parse overlay - allow trailing commas for partial JSON
		using var overlayDoc = JsonDocument.Parse(overlayJson.ToArray(), new JsonDocumentOptions
		{
			AllowTrailingCommas = true
		});

		using var writer2 = new Utf8JsonWriter(_outputBuffer, new JsonWriterOptions
		{
			Indented = false
		});

		MergeElements(writer2, _baseDocument.RootElement, overlayDoc.RootElement);
		writer2.Flush();
	}

	/// <summary>
	/// Deserializes the merged result to a specified type.
	/// </summary>
	/// <typeparam name="T">The type to deserialize to.</typeparam>
	/// <param name="options">Optional JSON serializer options.</param>
	/// <returns>The deserialized object, or default if deserialization fails.</returns>
	public T? Deserialize<T>(JsonSerializerOptions? options = null)
	{
		if (_outputBuffer.WrittenCount == 0)
			return default;

		try
		{
			return JsonSerializer.Deserialize<T>(_outputBuffer.WrittenSpan, options);
		}
		catch (JsonException)
		{
			return default;
		}
	}

	/// <summary>
	/// Recursively merges two JSON elements.
	/// </summary>
	private static void MergeElements(Utf8JsonWriter writer, JsonElement baseElement, JsonElement overlayElement)
	{
		// If types don't match, prefer the overlay
		if (baseElement.ValueKind != overlayElement.ValueKind)
		{
			WriteElement(writer, overlayElement);
			return;
		}

		switch (baseElement.ValueKind)
		{
			case JsonValueKind.Object:
				MergeObjects(writer, baseElement, overlayElement);
				break;

			case JsonValueKind.Array:
				MergeArrays(writer, baseElement, overlayElement);
				break;

			default:
				// For primitives (string, number, bool, null), use overlay value
				WriteElement(writer, overlayElement);
				break;
		}
	}

	/// <summary>
	/// Merges two JSON objects, combining properties from both.
	/// </summary>
	private static void MergeObjects(Utf8JsonWriter writer, JsonElement baseObj, JsonElement overlayObj)
	{
		writer.WriteStartObject();

		// Build a set of overlay property names for quick lookup
		var overlayProperties = new Dictionary<string, JsonElement>();
		foreach (var prop in overlayObj.EnumerateObject())
		{
			overlayProperties[prop.Name] = prop.Value;
		}

		// Process all base properties
		foreach (var baseProp in baseObj.EnumerateObject())
		{
			writer.WritePropertyName(baseProp.Name);

			if (overlayProperties.TryGetValue(baseProp.Name, out var overlayValue))
			{
				// Property exists in both - merge recursively
				MergeElements(writer, baseProp.Value, overlayValue);
				overlayProperties.Remove(baseProp.Name);
			}
			else
			{
				// Property only in base - keep it
				WriteElement(writer, baseProp.Value);
			}
		}

		// Add any remaining overlay properties not in base
		foreach (var (name, value) in overlayProperties)
		{
			writer.WritePropertyName(name);
			WriteElement(writer, value);
		}

		writer.WriteEndObject();
	}

	/// <summary>
	/// Merges two JSON arrays by index position.
	/// </summary>
	private static void MergeArrays(Utf8JsonWriter writer, JsonElement baseArray, JsonElement overlayArray)
	{
		writer.WriteStartArray();

		var baseLength = baseArray.GetArrayLength();
		var overlayLength = overlayArray.GetArrayLength();
		var maxLength = Math.Max(baseLength, overlayLength);

		for (int i = 0; i < maxLength; i++)
		{
			if (i < baseLength && i < overlayLength)
			{
				// Both have element at this index - merge
				MergeElements(writer, baseArray[i], overlayArray[i]);
			}
			else if (i < overlayLength)
			{
				// Only overlay has element - use it
				WriteElement(writer, overlayArray[i]);
			}
			else
			{
				// Only base has element - keep it
				WriteElement(writer, baseArray[i]);
			}
		}

		writer.WriteEndArray();
	}

	/// <summary>
	/// Writes a JSON element to the writer.
	/// </summary>
	private static void WriteElement(Utf8JsonWriter writer, JsonElement element)
	{
		switch (element.ValueKind)
		{
			case JsonValueKind.Object:
				writer.WriteStartObject();
				foreach (var prop in element.EnumerateObject())
				{
					writer.WritePropertyName(prop.Name);
					WriteElement(writer, prop.Value);
				}
				writer.WriteEndObject();
				break;

			case JsonValueKind.Array:
				writer.WriteStartArray();
				foreach (var item in element.EnumerateArray())
				{
					WriteElement(writer, item);
				}
				writer.WriteEndArray();
				break;

			case JsonValueKind.String:
				writer.WriteStringValue(element.GetString());
				break;

			case JsonValueKind.Number:
				if (element.TryGetInt64(out var longVal))
					writer.WriteNumberValue(longVal);
				else if (element.TryGetDouble(out var doubleVal))
					writer.WriteNumberValue(doubleVal);
				else
					writer.WriteNumberValue(element.GetDecimal());
				break;

			case JsonValueKind.True:
				writer.WriteBooleanValue(true);
				break;

			case JsonValueKind.False:
				writer.WriteBooleanValue(false);
				break;

			case JsonValueKind.Null:
				writer.WriteNullValue();
				break;
		}
	}
}
