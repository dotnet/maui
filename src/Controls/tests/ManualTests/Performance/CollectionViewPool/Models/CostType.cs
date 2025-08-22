using System.Collections.Generic;
using Newtonsoft.Json;

namespace PoolMath.Data;

public class CostTypeGroup : List<CostType>
{
	public CostTypeGroup(CostTypeCategory category, IEnumerable<CostType> items)
	{
		Name = Costs.GetCategory(category);
		Description = string.Empty;
		AddRange(items);
	}

	public CostTypeGroup(CostTypeCategory category, string description, IEnumerable<CostType> items)
	{
		Name = Costs.GetCategory(category);
		Description = description;
		AddRange(items);
	}

	public string Name { get; set; }

	public string Description { get; set; }
}

public class CostType : BaseDocument
{
	public CostType()
	{
	}

	public CostType(string id, string name, CostTypeCategory category, bool trackAmount = false)
	{
		Id = id;
		Name = name;
		Category = category;
		TrackAmount = trackAmount;
	}

	[JsonProperty("name")]
	[System.Text.Json.Serialization.JsonPropertyName("name")]
	public string Name { get; set; }

	[JsonProperty("category")]
	[System.Text.Json.Serialization.JsonPropertyName("category")]
	public CostTypeCategory Category { get; set; }

	[JsonProperty("trackAmount")]
	[System.Text.Json.Serialization.JsonPropertyName("trackAmount")]
	public bool TrackAmount { get; set; } = false;

	[JsonProperty("measurementUnits")]
	[System.Text.Json.Serialization.JsonPropertyName("measurementUnits")]
	public List<string> MeasurementUnits { get; set; } = new List<string>();

	[JsonProperty("measurementType")]
	[System.Text.Json.Serialization.JsonPropertyName("measurementType")]
	public MeasurementType MeasurementType { get; set; } = MeasurementType.None;

	[JsonProperty("Type")]
	[System.Text.Json.Serialization.JsonPropertyName("Type")]
	public override string Type => "costtype";
}
