using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PoolMath.Data;

public class SjTimelineConverter : JsonConverter<Log>
{
	public override bool CanConvert(Type objectType) =>
		objectType == typeof(Log);

	public override Log Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		// Check for null values
		if (reader.TokenType == JsonTokenType.Null)
			return null;

		// Copy the current state from reader (it's a struct)
		var readerAtStart = reader;

		// Read the `className` from our JSON document
		using var jsonDocument = JsonDocument.ParseValue(ref reader);
		var jsonObject = jsonDocument.RootElement;

		var typeName = jsonObject.GetProperty("type").GetString();

		return typeName switch
		{
			TestLog.TYPE_NAME => JsonSerializer.Deserialize<TestLog>(ref readerAtStart, options),
			MaintenanceLog.TYPE_NAME => JsonSerializer.Deserialize<MaintenanceLog>(ref readerAtStart, options),
			ChemicalLog.TYPE_NAME => JsonSerializer.Deserialize<ChemicalLog>(ref readerAtStart, options),
			NoteLog.TYPE_NAME => JsonSerializer.Deserialize<NoteLog>(ref readerAtStart, options),
			WeatherLog.TYPE_NAME => JsonSerializer.Deserialize<WeatherLog>(ref readerAtStart, options),
			CostLog.TYPE_NAME => JsonSerializer.Deserialize<CostLog>(ref readerAtStart, options),
			PumpScheduleChangeLog.TYPE_NAME => JsonSerializer.Deserialize<PumpScheduleChangeLog>(ref readerAtStart, options),
			_ => null
		};
	}

	public override void Write(Utf8JsonWriter writer, Log value, JsonSerializerOptions options)
	{
		// No need for this one in our use case, but to just dump the object into JSON
		// (without having the className property!), we can do this:
		JsonSerializer.Serialize(writer, value, value.GetType(), options);
	}
}
