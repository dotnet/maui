using System;
using Newtonsoft.Json;

namespace PoolMath.Data;

public class UnixEpochDateTimeConverter : Newtonsoft.Json.Converters.DateTimeConverterBase
{
	static readonly DateTime epochDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		if (reader.TokenType != JsonToken.Integer)
			throw new Exception("Invalid Token");

		var s = (long)reader.Value;
		return epochDate + TimeSpan.FromSeconds(s);
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		var d = ((DateTime)value) - epochDate;

		writer.WriteValue((long)d.TotalSeconds);
	}
}
