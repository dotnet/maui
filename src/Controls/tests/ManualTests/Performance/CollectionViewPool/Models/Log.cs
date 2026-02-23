using System;
using Newtonsoft.Json;

namespace PoolMath.Data;

[JsonConverter(typeof(TimelineConverter))]
[System.Text.Json.Serialization.JsonConverter(typeof(SjTimelineConverter))]
public abstract class Log : BaseDocument
{
	public Log()
	{
		LogTimestamp = DateTime.UtcNow;
	}

	[JsonIgnore]
	[System.Text.Json.Serialization.JsonIgnore]
	public abstract string ApiRoute { get; }

	[JsonProperty("poolId")]
	[System.Text.Json.Serialization.JsonPropertyName("poolId")]
	public string PoolId { get; set; }

	[JsonProperty("logTimestamp")]
	[System.Text.Json.Serialization.JsonPropertyName("logTimestamp")]
	public DateTime LogTimestamp { get; set; }

	[JsonProperty("weather")]
	[System.Text.Json.Serialization.JsonPropertyName("weather")]
	public WeatherSummary Weather { get; set; }

	[JsonProperty("weatherLogId")]
	[System.Text.Json.Serialization.JsonPropertyName("weatherLogId")]
	public string WeatherLogId { get; set; }

	public static bool IsLogType<T>(T instance) where T : Log
	{
		var t = typeof(T);
		return t == typeof(TestLog)
			|| t == typeof(ChemicalLog)
			|| t == typeof(CostLog)
			|| t == typeof(MaintenanceLog)
			|| t == typeof(NoteLog);
	}

	public static bool IsLogType(string typeName)
		=> typeName == TestLog.TYPE_NAME
			|| typeName == ChemicalLog.TYPE_NAME
			|| typeName == CostLog.TYPE_NAME
			|| typeName == MaintenanceLog.TYPE_NAME
			|| typeName == NoteLog.TYPE_NAME
			|| typeName == PumpScheduleChangeLog.TYPE_NAME;

	public static string GetLogType<TLog>() where TLog : Log
	{
		var t = typeof(TLog);
		if (t == typeof(TestLog))
			return TestLog.TYPE_NAME;
		if (t == typeof(ChemicalLog))
			return ChemicalLog.TYPE_NAME;
		if (t == typeof(CostLog))
			return CostLog.TYPE_NAME;
		if (t == typeof(MaintenanceLog))
			return MaintenanceLog.TYPE_NAME;
		if (t == typeof(NoteLog))
			return NoteLog.TYPE_NAME;
		if (t == typeof(PumpScheduleChangeLog))
			return PumpScheduleChangeLog.TYPE_NAME;

		return null;
	}
}
