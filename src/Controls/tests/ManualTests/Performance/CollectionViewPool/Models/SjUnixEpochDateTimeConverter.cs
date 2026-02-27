using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PoolMath.Data;

public class SjUnixEpochDateTimeConverter : JsonConverter<DateTime>
{
	static readonly DateTime epochDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

	public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.Number)
			throw new Exception("Invalid Date");

		var s = reader.GetInt64();
		return epochDate + TimeSpan.FromSeconds(s);
	}

	public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
	{
		var d = ((DateTime)value) - epochDate;

		writer.WriteNumberValue((long)d.TotalSeconds);
	}
}
