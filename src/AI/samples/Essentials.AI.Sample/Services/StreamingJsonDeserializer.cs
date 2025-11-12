using System.Buffers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Maui.Controls.Sample.Services;

/// <summary>
/// Zero-allocation streaming JSON deserializer using Utf8JsonWriter with ReadOnlySpan<byte> overloads.
/// Eliminates string allocations by using the writer's native UTF-8 span APIs.
/// </summary>
internal sealed class StreamingJsonDeserializer<T>
	where T : class
{
	private readonly ArrayBufferWriter<byte> _byteBuffer = new(initialCapacity: 4096);
	private readonly ArrayBufferWriter<byte> _reconstructionBuffer = new(initialCapacity: 4096);
	private readonly Stack<byte> _bracketStack = new();

	private readonly JsonSerializerOptions _options;
	private readonly bool _skipDeserialization;
	private T? _lastGoodModel;

	/// <summary>
	/// Initializes a new instance of the StreamingJsonDeserializer.
	/// </summary>
	/// <param name="options">Custom JSON serialization options, or null to use defaults.</param>
	/// <param name="skipDeserialization">If true, reconstructs JSON without deserializing to the target type (useful for validation).</param>
	public StreamingJsonDeserializer(JsonSerializerOptions? options = null, bool skipDeserialization = false)
	{
		_skipDeserialization = skipDeserialization;
		_options = options ?? new JsonSerializerOptions
		{
			PropertyNameCaseInsensitive = true,
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
			AllowTrailingCommas = true,
			ReadCommentHandling = JsonCommentHandling.Skip,
			Converters = { new JsonStringEnumConverter() },

			// Make all properties optional to handle incomplete streaming JSON
			TypeInfoResolver = new DefaultJsonTypeInfoResolver
			{
				Modifiers =
				{
					static typeInfo =>
					{
						if (typeInfo.Kind != JsonTypeInfoKind.Object)
							return;

						foreach (var propertyInfo in typeInfo.Properties)
						{
							propertyInfo.IsRequired = false;
						}
					}
				}
			}
		};
	}

	/// <summary>
	/// Gets the current accumulated JSON as UTF-8 bytes (zero-allocation view).
	/// </summary>
	public ReadOnlySpan<byte> PartialJsonUtf8 => _byteBuffer.WrittenSpan;

	/// <summary>
	/// Gets the current accumulated JSON as a string (allocates for string conversion).
	/// </summary>
	public string PartialJson => Encoding.UTF8.GetString(_byteBuffer.WrittenSpan);

	/// <summary>
	/// Gets the last successfully deserialized model, or null if none have succeeded yet.
	/// </summary>
	public T? LastGoodModel => _lastGoodModel;

	/// <summary>
	/// Processes a new chunk of JSON text from a streaming source.
	/// Accumulates the chunk into the internal buffer and attempts to deserialize.
	/// </summary>
	/// <param name="chunk">The incoming JSON text fragment to process.</param>
	/// <returns>The most recently successfully deserialized model, or null if none have succeeded yet.</returns>
	public T? ProcessChunk(string chunk)
	{
		if (string.IsNullOrEmpty(chunk))
			return _lastGoodModel;

		// Convert the incoming chunk to UTF-8 bytes and append to the buffer
		var byteCount = Encoding.UTF8.GetByteCount(chunk);
		var span = _byteBuffer.GetSpan(byteCount);
		Encoding.UTF8.GetBytes(chunk, span);
		_byteBuffer.Advance(byteCount);

		// Attempt to deserialize the accumulated JSON so far
		var parsed = TryDeserializeIncremental();
		if (parsed != null)
		{
			_lastGoodModel = parsed;
		}

		return _lastGoodModel;
	}

	/// <summary>
	/// Resets the deserializer state, clearing all accumulated data and cached models.
	/// Call this to start processing a new stream from scratch.
	/// </summary>
	public void Reset()
	{
		_byteBuffer.Clear();
		_reconstructionBuffer.Clear();
		_bracketStack.Clear();
		_lastGoodModel = default;
	}

	/// <summary>
	/// Attempts to reconstruct and deserialize the accumulated JSON buffer.
	/// Handles incomplete JSON by closing unclosed structures and extracting partial string values.
	/// </summary>
	/// <returns>A deserialized model instance if successful, otherwise null.</returns>
	private T? TryDeserializeIncremental()
	{
		var bytes = _byteBuffer.WrittenSpan;
		if (bytes.IsEmpty)
			return null;

		try
		{
			// Reconstruct valid JSON from potentially incomplete input
			var reconstructedBytes = ReconstructValidJsonWithUtf8Writer(bytes);

			if (reconstructedBytes.Length > 0)
			{
				if (_skipDeserialization)
					return null;

				try
				{
					return JsonSerializer.Deserialize<T>(reconstructedBytes, _options);
				}
				catch (JsonException)
				{
					// Deserialization failed - JSON may still be too incomplete
					return null;
				}
			}
		}
		catch
		{
			// Reconstruction failed
		}

		return null;
	}

	/// <summary>
	/// Reconstructs valid, complete JSON from potentially incomplete streaming JSON input.
	/// Uses Utf8JsonReader to parse tokens and Utf8JsonWriter to rebuild valid JSON structure.
	/// Handles incomplete values by completing partial strings and closing unclosed brackets/braces.
	/// </summary>
	/// <param name="incompleteUtf8Json">The potentially incomplete UTF-8 JSON bytes to reconstruct.</param>
	/// <returns>A span containing complete, valid UTF-8 JSON bytes.</returns>
	private ReadOnlySpan<byte> ReconstructValidJsonWithUtf8Writer(ReadOnlySpan<byte> incompleteUtf8Json)
	{
		_reconstructionBuffer.Clear();
		_bracketStack.Clear();

		// Use non-final block mode to allow incomplete JSON
		var reader = new Utf8JsonReader(incompleteUtf8Json, isFinalBlock: false, state: default);
		var writer = new Utf8JsonWriter(_reconstructionBuffer);

		// Track pending property name for key-value pairs
		ReadOnlySpan<byte> pendingPropertyNameBytes = default;
		var lastTokenStart = 0;

		try
		{
			while (reader.Read())
			{
				// Track position for partial string extraction later
				lastTokenStart = (int)reader.TokenStartIndex;

				switch (reader.TokenType)
				{
					case JsonTokenType.StartObject:
						WritePendingPropertyName(writer, ref pendingPropertyNameBytes);
						writer.WriteStartObject();
						// Track that this object needs closing
						_bracketStack.Push((byte)'}');
						break;

					case JsonTokenType.EndObject:
						// Only close if we have a matching open brace
						if (_bracketStack.Count > 0 && _bracketStack.Peek() == (byte)'}')
						{
							_bracketStack.Pop();
							writer.WriteEndObject();
						}
						break;

					case JsonTokenType.StartArray:
						WritePendingPropertyName(writer, ref pendingPropertyNameBytes);
						writer.WriteStartArray();
						// Track that this array needs closing
						_bracketStack.Push((byte)']');
						break;

					case JsonTokenType.EndArray:
						// Only close if we have a matching open bracket
						if (_bracketStack.Count > 0 && _bracketStack.Peek() == (byte)']')
						{
							_bracketStack.Pop();
							writer.WriteEndArray();
						}
						break;

					case JsonTokenType.PropertyName:
						// Store property name bytes to write later when we get the value
						pendingPropertyNameBytes = reader.HasValueSequence
							? reader.ValueSequence.ToArray()
							: reader.ValueSpan;
						break;

					case JsonTokenType.String:
						WriteStringValue(writer, ref reader, ref pendingPropertyNameBytes);
						break;

					case JsonTokenType.Number:
						WriteNumberValue(writer, ref reader, ref pendingPropertyNameBytes);
						break;

					case JsonTokenType.True:
						WriteBooleanValue(writer, true, ref pendingPropertyNameBytes);
						break;

					case JsonTokenType.False:
						WriteBooleanValue(writer, false, ref pendingPropertyNameBytes);
						break;

					case JsonTokenType.Null:
						WriteNullValue(writer, ref pendingPropertyNameBytes);
						break;
				}
			}
		}
		catch
		{
			// Reader exhausted - expected for incomplete JSON
		}

		// If we have a property name without a value, extract the partial string value
		if (!pendingPropertyNameBytes.IsEmpty)
		{
			TryWritePartialStringValue(writer, incompleteUtf8Json, lastTokenStart, pendingPropertyNameBytes);
		}

		// Close any unclosed JSON structures (objects/arrays) to make the JSON valid
		while (_bracketStack.Count > 0)
		{
			var bracket = _bracketStack.Pop();
			if (bracket == (byte)'}')
				writer.WriteEndObject();
			else if (bracket == (byte)']')
				writer.WriteEndArray();
		}

		writer.Flush();
		return _reconstructionBuffer.WrittenSpan;
	}

	/// <summary>
	/// Writes a pending property name to the JSON writer if one exists, then clears it.
	/// Used before writing values or nested structures.
	/// </summary>
	private static void WritePendingPropertyName(Utf8JsonWriter writer, ref ReadOnlySpan<byte> pendingPropertyNameBytes)
	{
		if (!pendingPropertyNameBytes.IsEmpty)
		{
			// Use span-based overload for zero-allocation property name writing
			writer.WritePropertyName(pendingPropertyNameBytes);
			pendingPropertyNameBytes = default;
		}
	}

	/// <summary>
	/// Writes a string value from the reader to the writer.
	/// Handles both property values (with pending property name) and array elements (without).
	/// </summary>
	private static void WriteStringValue(Utf8JsonWriter writer, ref Utf8JsonReader reader, ref ReadOnlySpan<byte> pendingPropertyNameBytes)
	{
		ReadOnlySpan<byte> stringBytes = GetStringBytes(ref reader);

		if (!pendingPropertyNameBytes.IsEmpty)
		{
			// Write as property: "propertyName": "value"
			writer.WriteString(pendingPropertyNameBytes, stringBytes);
			pendingPropertyNameBytes = default;
		}
		else
		{
			// Write as standalone value (array element or root value)
			writer.WriteStringValue(stringBytes);
		}
	}

	/// <summary>
	/// Extracts string bytes from the JSON reader, handling escape sequences if present.
	/// </summary>
	/// <returns>UTF-8 bytes representing the unescaped string value.</returns>
	private static ReadOnlySpan<byte> GetStringBytes(ref Utf8JsonReader reader)
	{
		if (reader.ValueIsEscaped)
		{
			// String contains escape sequences - need to unescape them
			var unescapedBuffer = new ArrayBufferWriter<byte>();
			var span = unescapedBuffer.GetSpan(reader.HasValueSequence ? (int)reader.ValueSequence.Length : reader.ValueSpan.Length);
			int written = reader.CopyString(span);
			unescapedBuffer.Advance(written);
			return unescapedBuffer.WrittenSpan;
		}

		// No escaping - return raw bytes (handle both contiguous and segmented buffers)
		return reader.HasValueSequence ? reader.ValueSequence.ToArray() : reader.ValueSpan;
	}

	/// <summary>
	/// Writes a numeric value from the reader to the writer.
	/// Handles both property values and array elements, choosing the most appropriate numeric type (long, double, or decimal).
	/// </summary>
	private static void WriteNumberValue(Utf8JsonWriter writer, ref Utf8JsonReader reader, ref ReadOnlySpan<byte> pendingPropertyNameBytes)
	{
		if (!pendingPropertyNameBytes.IsEmpty)
		{
			// Write as property: "propertyName": 123
			// Try integer first, then floating point, finally decimal for precision
			if (reader.TryGetInt64(out var longValue))
				writer.WriteNumber(pendingPropertyNameBytes, longValue);
			else if (reader.TryGetDouble(out var doubleValue))
				writer.WriteNumber(pendingPropertyNameBytes, doubleValue);
			else
				writer.WriteNumber(pendingPropertyNameBytes, reader.GetDecimal());
			
			pendingPropertyNameBytes = default;
		}
		else
		{
			// Write as standalone value
			if (reader.TryGetInt64(out var longValue))
				writer.WriteNumberValue(longValue);
			else if (reader.TryGetDouble(out var doubleValue))
				writer.WriteNumberValue(doubleValue);
			else
				writer.WriteNumberValue(reader.GetDecimal());
		}
	}

	/// <summary>
	/// Writes a boolean value to the writer.
	/// Handles both property values and array elements.
	/// </summary>
	private static void WriteBooleanValue(Utf8JsonWriter writer, bool value, ref ReadOnlySpan<byte> pendingPropertyNameBytes)
	{
		if (!pendingPropertyNameBytes.IsEmpty)
		{
			// Write as property: "propertyName": true/false
			writer.WriteBoolean(pendingPropertyNameBytes, value);
			pendingPropertyNameBytes = default;
		}
		else
		{
			// Write as standalone value
			writer.WriteBooleanValue(value);
		}
	}

	/// <summary>
	/// Writes a null value to the writer.
	/// Handles both property values and array elements.
	/// </summary>
	private static void WriteNullValue(Utf8JsonWriter writer, ref ReadOnlySpan<byte> pendingPropertyNameBytes)
	{
		if (!pendingPropertyNameBytes.IsEmpty)
		{
			// Write as property: "propertyName": null
			writer.WriteNull(pendingPropertyNameBytes);
			pendingPropertyNameBytes = default;
		}
		else
		{
			// Write as standalone value
			writer.WriteNullValue();
		}
	}

	/// <summary>
	/// Attempts to extract and write a partially received string value for a pending property.
	/// This handles cases where streaming JSON cuts off mid-string (e.g., "name": "John Do[end of chunk]).
	/// </summary>
	/// <param name="writer">The JSON writer to write the completed property to.</param>
	/// <param name="utf8Json">The complete accumulated JSON buffer.</param>
	/// <param name="lastPosition">The position of the last successfully parsed token.</param>
	/// <param name="propertyNameBytes">The UTF-8 bytes of the property name.</param>
	private static void TryWritePartialStringValue(Utf8JsonWriter writer, ReadOnlySpan<byte> utf8Json, int lastPosition, ReadOnlySpan<byte> propertyNameBytes)
	{
		// Build search pattern: "propertyName":"
		Span<byte> pattern = stackalloc byte[propertyNameBytes.Length + 4];
		pattern[0] = (byte)'"';
		propertyNameBytes.CopyTo(pattern[1..^3]);
		pattern[^3] = (byte)'"';
		pattern[^2] = (byte)':';
		pattern[^1] = (byte)'"';

		// Find the last occurrence of the property:value pattern
		var index = utf8Json.LastIndexOf(pattern);
		if (index < 0)
			return;

		// Extract everything after the opening quote of the string value
		var valueStartIndex = index + pattern.Length;
		if (valueStartIndex >= utf8Json.Length)
			return;

		// Unescape any escape sequences in the partial value
		var partialValueBytes = utf8Json[valueStartIndex..];
		var unescapedBytes = UnescapeJsonStringBytes(partialValueBytes);

		writer.WriteString(propertyNameBytes, unescapedBytes.WrittenSpan);
	}

	/// <summary>
	/// Unescapes JSON string escape sequences (like \n, \t, \uXXXX) in UTF-8 bytes.
	/// Handles standard escape sequences and Unicode escape sequences.
	/// </summary>
	/// <param name="escapedBytes">The UTF-8 bytes containing escape sequences.</param>
	/// <returns>A buffer containing the unescaped UTF-8 bytes.</returns>
	private static ArrayBufferWriter<byte> UnescapeJsonStringBytes(ReadOnlySpan<byte> escapedBytes)
	{
		// Remove trailing backslash if present (incomplete escape sequence)
		if (escapedBytes[^1] == (byte)'\\')
			escapedBytes = escapedBytes[..^1];
			
		var buffer = new ArrayBufferWriter<byte>(escapedBytes.Length);

		for (int i = 0; i < escapedBytes.Length; i++)
		{
			if (escapedBytes[i] == (byte)'\\' && i + 1 < escapedBytes.Length)
			{
				// Process escape sequences
				switch (escapedBytes[i + 1])
				{
					case (byte)'"':
					case (byte)'\\':
					case (byte)'/':
						// Simple character escapes - write the escaped character directly
						buffer.GetSpan(1)[0] = escapedBytes[i + 1];
						buffer.Advance(1);
						i++; // Skip the escape sequence
						break;
					case (byte)'b':
						buffer.GetSpan(1)[0] = (byte)'\b';
						buffer.Advance(1);
						i++;
						break;
					case (byte)'f':
						buffer.GetSpan(1)[0] = (byte)'\f';
						buffer.Advance(1);
						i++;
						break;
					case (byte)'n':
						buffer.GetSpan(1)[0] = (byte)'\n';
						buffer.Advance(1);
						i++;
						break;
					case (byte)'r':
						buffer.GetSpan(1)[0] = (byte)'\r';
						buffer.Advance(1);
						i++;
						break;
					case (byte)'t':
						buffer.GetSpan(1)[0] = (byte)'\t';
						buffer.Advance(1);
						i++;
						break;
					case (byte)'u' when i + 5 < escapedBytes.Length:
						// Unicode escape sequence: \uXXXX (4 hex digits)
						if (TryParseHexToChar(escapedBytes.Slice(i + 2, 4), out char unicodeChar))
						{
							// Convert the Unicode character to UTF-8 bytes directly into the buffer
							var unichars = new ReadOnlySpan<char>(in unicodeChar);
							var bufferBytes = buffer.GetSpan(4);
							var bytesWritten = Encoding.UTF8.GetBytes(unichars, bufferBytes);
							buffer.Advance(bytesWritten);
							i += 5; // Skip \uXXXX
						}
						else
						{
							// Invalid hex - write the backslash as-is
							buffer.GetSpan(1)[0] = escapedBytes[i];
							buffer.Advance(1);
						}
						break;
					default:
						// Unknown escape - write the backslash as-is
						buffer.GetSpan(1)[0] = escapedBytes[i];
						buffer.Advance(1);
						break;
				}
			}
			else
			{
				// Regular character - copy as-is
				buffer.GetSpan(1)[0] = escapedBytes[i];
				buffer.Advance(1);
			}
		}

		return buffer;
	}

	/// <summary>
	/// Parses a 4-character hexadecimal sequence (like "00A9" for ©) into a Unicode character.
	/// Used for handling \uXXXX escape sequences in JSON strings.
	/// </summary>
	/// <param name="hexBytes">UTF-8 bytes representing 4 hexadecimal digits.</param>
	/// <param name="result">The resulting Unicode character if parsing succeeded.</param>
	/// <returns>True if parsing succeeded, false if the hex sequence is invalid.</returns>
	private static bool TryParseHexToChar(ReadOnlySpan<byte> hexBytes, out char result)
	{
		result = '\0';
		if (hexBytes.Length != 4)
			return false;

		int value = 0;
		foreach (byte b in hexBytes)
		{
			// Convert ASCII hex digit to numeric value (0-15)
			int hexDigit = b switch
			{
				>= (byte)'0' and <= (byte)'9' => b - '0',
				>= (byte)'A' and <= (byte)'F' => b - 'A' + 10,
				>= (byte)'a' and <= (byte)'f' => b - 'a' + 10,
				_ => -1
			};

			if (hexDigit < 0)
				return false;

			// Shift previous value left by 4 bits (one hex digit) and OR in the new digit
			value = (value << 4) | hexDigit;
		}

		result = (char)value;
		return true;
	}
}
