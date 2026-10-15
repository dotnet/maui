using System.ComponentModel;
using System.Text.Json;

namespace Microsoft.Maui.Controls.HybridWebApps;

/// <summary>Compiled collection loops used with generated typed element adapters.</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class HybridJsonCollections
{
	/// <summary>Reads an array into an explicitly created collection.</summary>
	/// <typeparam name="T">The element contract.</typeparam><typeparam name="TCollection">The concrete collection.</typeparam>
	/// <param name="json">The input array.</param><param name="create">The typed constructor.</param>
	/// <param name="read">The strict element reader.</param><returns>The collection.</returns>
	public static TCollection Read<T, TCollection>(JsonElement json, Func<TCollection> create, Func<JsonElement, T> read)
		where TCollection : ICollection<T>
	{
		if (json.ValueKind != JsonValueKind.Array)
			throw new JsonException("Expected an array.");
		var result = create();
		foreach (var item in json.EnumerateArray())
			result.Add(read(item));
		return result;
	}

	/// <summary>Reads a dictionary, rejecting duplicate keys.</summary>
	/// <typeparam name="T">The value contract.</typeparam><param name="json">The input object.</param>
	/// <param name="read">The strict value reader.</param><returns>An ordinal dictionary.</returns>
	public static Dictionary<string, T> ReadDictionary<T>(JsonElement json, Func<JsonElement, T> read)
	{
		if (json.ValueKind != JsonValueKind.Object)
			throw new JsonException("Expected an object.");
		var result = new Dictionary<string, T>(StringComparer.Ordinal);
		foreach (var item in json.EnumerateObject())
			if (!result.TryAdd(item.Name, read(item.Value)))
				throw new JsonException("Duplicate key.");
		return result;
	}

	/// <summary>Writes a sequence using its strict typed element writer.</summary>
	/// <typeparam name="T">The element contract.</typeparam><param name="writer">The output.</param>
	/// <param name="values">The sequence.</param><param name="write">The generated element adapter.</param>
	public static void Write<T>(Utf8JsonWriter writer, IEnumerable<T> values, Action<Utf8JsonWriter, T> write)
	{
		writer.WriteStartArray();
		foreach (var value in values)
			write(writer, value);
		writer.WriteEndArray();
	}

	/// <summary>Writes a string-keyed dictionary using its strict typed value writer.</summary>
	/// <typeparam name="T">The value contract.</typeparam><param name="writer">The output.</param>
	/// <param name="values">The dictionary.</param><param name="write">The generated value adapter.</param>
	public static void WriteDictionary<T>(Utf8JsonWriter writer, IEnumerable<KeyValuePair<string, T>> values, Action<Utf8JsonWriter, T> write)
	{
		writer.WriteStartObject();
		foreach (var value in values)
		{ writer.WritePropertyName(value.Key); write(writer, value.Value); }
		writer.WriteEndObject();
	}
}
