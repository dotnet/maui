using Newtonsoft.Json;

namespace PoolMath.Data;

public class TestLog : Log
{
	public const string API_ROUTE = "testlogs";

	public const string TYPE_NAME = "testlog";

	[JsonIgnore]
	[System.Text.Json.Serialization.JsonIgnore]
	public override string ApiRoute => API_ROUTE;

	[JsonProperty("type")]
	[System.Text.Json.Serialization.JsonPropertyName("type")]
	public override string Type => TYPE_NAME;

	[JsonProperty("fc")]
	[System.Text.Json.Serialization.JsonPropertyName("fc")]
	public double? FreeChlorine { get; set; }

	[JsonProperty("cc")]
	[System.Text.Json.Serialization.JsonPropertyName("cc")]
	public double? CombinedChlorine { get; set; }

	[JsonProperty("cya")]
	[System.Text.Json.Serialization.JsonPropertyName("cya")]
	public double? Cya { get; set; }

	[JsonProperty("ch")]
	[System.Text.Json.Serialization.JsonPropertyName("ch")]
	public double? CalciumHardness { get; set; }

	[JsonProperty("ph")]
	[System.Text.Json.Serialization.JsonPropertyName("ph")]
	public double? Ph { get; set; }

	[JsonProperty("ta")]
	[System.Text.Json.Serialization.JsonPropertyName("ta")]
	public double? TotalAlkalinity { get; set; }

	[JsonProperty("salt")]
	[System.Text.Json.Serialization.JsonPropertyName("salt")]
	public double? Salt { get; set; }

	[JsonProperty("bor")]
	[System.Text.Json.Serialization.JsonPropertyName("bor")]
	public double? Borates { get; set; }

	[JsonProperty("tds")]
	[System.Text.Json.Serialization.JsonPropertyName("tds")]
	public double? TDS { get; set; }

	[JsonProperty("csi")]
	[System.Text.Json.Serialization.JsonPropertyName("csi")]
	public double? CSI { get; set; }

	[JsonProperty("waterTemp")]
	[System.Text.Json.Serialization.JsonPropertyName("waterTemp")]
	public double? WaterTemp { get; set; }

	[JsonProperty("waterTempUnits")]
	[System.Text.Json.Serialization.JsonPropertyName("waterTempUnits")]
	public Units? WaterTempUnits { get; set; } = null;
}
