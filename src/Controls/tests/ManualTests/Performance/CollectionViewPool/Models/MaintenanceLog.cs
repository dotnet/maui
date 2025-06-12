using Newtonsoft.Json;

namespace PoolMath.Data;

public class MaintenanceLog : Log
{
	public const string API_ROUTE = "maintenancelogs";

	public const string TYPE_NAME = "maintlog";

	[JsonIgnore]
	[System.Text.Json.Serialization.JsonIgnore]
	public override string ApiRoute => API_ROUTE;

	[JsonProperty("type")]
	[System.Text.Json.Serialization.JsonPropertyName("type")]
	public override string Type => TYPE_NAME;

	[JsonProperty("backwashed")]
	[System.Text.Json.Serialization.JsonPropertyName("backwashed")]
	public bool Backwashed { get; set; }

	[JsonProperty("brushed")]
	[System.Text.Json.Serialization.JsonPropertyName("brushed")]
	public bool Brushed { get; set; }

	[JsonProperty("cleanedFilter")]
	[System.Text.Json.Serialization.JsonPropertyName("cleanedFilter")]
	public bool CleanedFilter { get; set; }

	[JsonProperty("vacuumed")]
	[System.Text.Json.Serialization.JsonPropertyName("vacuumed")]
	public bool Vacuumed { get; set; }

	[JsonProperty("flowRate")]
	[System.Text.Json.Serialization.JsonPropertyName("flowRate")]
	public double? FlowRate { get; set; }

	[JsonProperty("pressure")]
	[System.Text.Json.Serialization.JsonPropertyName("pressure")]
	public double? Pressure { get; set; }

	[JsonProperty("waterTemp")]
	[System.Text.Json.Serialization.JsonPropertyName("waterTemp")]
	public double? WaterTemp { get; set; }

	[JsonProperty("pumpRuntime")]
	[System.Text.Json.Serialization.JsonPropertyName("pumpRuntime")]
	public int? PumpRunTime { get; set; }

	[JsonProperty("swgCellPercent")]
	[System.Text.Json.Serialization.JsonPropertyName("swgCellPercent")]
	public int? SWGCellPercent { get; set; }

	[JsonProperty("opened")]
	[System.Text.Json.Serialization.JsonPropertyName("opened")]
	public bool Opened { get; set; }

	[JsonProperty("closed")]
	[System.Text.Json.Serialization.JsonPropertyName("closed")]
	public bool Closed { get; set; }

	[JsonProperty("notes")]
	[System.Text.Json.Serialization.JsonPropertyName("notes")]
	public string Notes { get; set; }

	//[JsonIgnore]
	//public bool ShowFlowRate => FlowRate.HasValue && (Pool?.TrackFlowRate ?? true);
	//[JsonIgnore]
	//public bool ShowPressure => Pressure.HasValue && (Pool?.TrackPressure ?? true);
	//[JsonIgnore]
	//public bool ShowWaterTemp => WaterTemp.HasValue && (Pool?.TrackWaterTemp ?? true);
	//[JsonIgnore]
	//public bool ShowPumpRunTime => PumpRunTime.HasValue && (Pool?.TrackPumpRunTime ?? true);
	//[JsonIgnore]
	//public bool ShowSWGCellPercent => SWGCellPercent.HasValue && (Pool?.TrackSWGCellPercent ?? true);
}
