using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace PoolMath.Data;

public class CostLog : Log
{
	public const string API_ROUTE = "costlogs";

	public const string TYPE_NAME = "costlog";

	[JsonIgnore]
	[System.Text.Json.Serialization.JsonIgnore]
	public override string ApiRoute => API_ROUTE;

	[JsonProperty("type")]
	[System.Text.Json.Serialization.JsonPropertyName("type")]
	public override string Type => TYPE_NAME;

	[JsonProperty("bleachPercent")]
	[System.Text.Json.Serialization.JsonPropertyName("bleachPercent")]
	public double? BleachPercent { get; set; }

	[JsonProperty("measurementUnit")]
	[System.Text.Json.Serialization.JsonPropertyName("measurementUnit")]
	public string MeasurementUnit { get; set; }

	[JsonProperty("amount")]
	[System.Text.Json.Serialization.JsonPropertyName("amount")]
	public double? Amount { get; set; }

	[JsonProperty("normalizedAmount")]
	[System.Text.Json.Serialization.JsonPropertyName("normalizedAmount")]
	public double? NormalizedAmount { get; set; }

	[JsonProperty("cost")]
	[System.Text.Json.Serialization.JsonPropertyName("cost")]
	public double? Cost { get; set; }

	[JsonProperty("costTypeId")]
	[System.Text.Json.Serialization.JsonPropertyName("costTypeId")]
	public string CostTypeId { get; set; }

	[JsonProperty("description")]
	[System.Text.Json.Serialization.JsonPropertyName("description")]
	public string Description { get; set; }

	public void NormalizeAmount(IEnumerable<CostType> availableTypes)
	{
	}
}

public enum CostTypeCategory
{
	Other = 0,
	Chemicals_Sanitizer = 10,
	Chemicals_PhUp = 20,
	Chemicals_PhDown = 30,
	Chemicals_Ta = 40,
	Chemicals_Ch = 50,
	Chemicals_Stabilizer = 60,
	Chemicals_Bor = 70,
	Chemicals_Misc = 90,
	Utilities = 100,
	Equipment = 200,
}
