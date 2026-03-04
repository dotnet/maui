using Newtonsoft.Json;

namespace PoolMath.Data;

public abstract class BaseDocument : BaseModel
{
	[JsonProperty("type")]
	[System.Text.Json.Serialization.JsonPropertyName("type")]
	public abstract string Type { get; }

	[JsonProperty("userId")]
	[System.Text.Json.Serialization.JsonPropertyName("userId")]
	public string UserId { get; set; }

	[JsonProperty("origin")]
	[System.Text.Json.Serialization.JsonPropertyName("origin")]
	public string Origin { get; set; }
}
