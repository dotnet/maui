using Newtonsoft.Json;

namespace PoolMath.Data;

public class ChemicalLog : Log
{
	public const string API_ROUTE = "chemlogs";

	public const string TYPE_NAME = "chemlog";

	[JsonIgnore]
	[System.Text.Json.Serialization.JsonIgnore]
	public override string ApiRoute => API_ROUTE;

	[JsonProperty("type")]
	[System.Text.Json.Serialization.JsonPropertyName("type")]
	public override string Type => TYPE_NAME;

	[System.Text.Json.Serialization.JsonPropertyName("Chemical")]
	public ChemicalAdditionType? Chemical { get; set; }

	[JsonProperty("runTime")]
	[System.Text.Json.Serialization.JsonPropertyName("runTime")]
	public double? RunTime { get; set; }

	[System.Text.Json.Serialization.JsonPropertyName("Percent")]
	public double? Percent { get; set; }

	[System.Text.Json.Serialization.JsonPropertyName("Unit")]
	public UnitOfMeasurement Unit { get; set; }

	[System.Text.Json.Serialization.JsonPropertyName("Amount")]
	public double? Amount { get; set; }

	[System.Text.Json.Serialization.JsonPropertyName("NormalizedAmount")]
	public double? NormalizedAmount { get; set; }

	public void NormalizeAmount()
	{
		//if (!Amount.HasValue)
		//	return;

		//NormalizedAmount = Calculator.NormalizeAmountToSmallest(Unit, Amount.Value);
	}
}
