using Newtonsoft.Json;

namespace PoolMath.Data;

public class NoteLog : Log
{
	public const string API_ROUTE = "notelogs";

	public const string TYPE_NAME = "notelog";

	[JsonIgnore]
	[System.Text.Json.Serialization.JsonIgnore]
	public override string ApiRoute => API_ROUTE;

	[JsonProperty("type")]
	[System.Text.Json.Serialization.JsonPropertyName("type")]
	public override string Type => TYPE_NAME;

	[JsonProperty("notes")]
	[System.Text.Json.Serialization.JsonPropertyName("notes")]
	public string Notes { get; set; }
}
