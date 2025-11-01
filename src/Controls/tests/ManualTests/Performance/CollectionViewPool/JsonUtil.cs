using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;

namespace PoolMath;

public static class JsonUtil
{
	public static JsonSerializerOptions SetOptions(JsonSerializerOptions options)
	{
		options.AllowTrailingCommas = true;
		options.IgnoreReadOnlyFields = true;
		options.IgnoreReadOnlyProperties = false;
		options.DictionaryKeyPolicy = null;
		options.PropertyNameCaseInsensitive = true;
		options.PropertyNamingPolicy = null;
		options.NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals;

		return options;
	}

	public readonly static System.Text.Json.JsonSerializerOptions SerializerOptions
		= SetOptions(new JsonSerializerOptions());

	[RequiresUnreferencedCode()]
	public static string Serialize<T>(T obj)
		=> JsonSerializer.Serialize<T>(obj, SerializerOptions);

	[RequiresUnreferencedCode()]
	public static T Deserialize<T>(string json)
		=> JsonSerializer.Deserialize<T>(json, SerializerOptions);

	public static bool TryDeserialize<T>(string json, out T result)
	{
		try
		{
			result = JsonSerializer.Deserialize<T>(json, SerializerOptions);
			return true;
		}
		catch
		{
			result = default;
			return false;
		}
	}

	public static void Serialize<T>(T obj, Stream outputStream)
		=> JsonSerializer.Serialize<T>(outputStream, obj, SerializerOptions);

	public static T Deserialize<T>(Stream inputStream)
		=> JsonSerializer.Deserialize<T>(inputStream, SerializerOptions);
}
