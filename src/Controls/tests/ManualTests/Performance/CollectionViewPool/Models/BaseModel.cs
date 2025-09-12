using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace PoolMath.Data;

public abstract class BaseModel
{
	[JsonProperty("id")]
	[System.Text.Json.Serialization.JsonPropertyName("id")]
	public string Id { get; set; }

	[JsonProperty("_ts")]
	[System.Text.Json.Serialization.JsonPropertyName("_ts")]
	[JsonConverter(typeof(UnixEpochDateTimeConverter))]
	[System.Text.Json.Serialization.JsonConverter(typeof(SjUnixEpochDateTimeConverter))]
	public DateTime Timestamp { get; set; }

	[JsonProperty("deleted")]
	[System.Text.Json.Serialization.JsonPropertyName("deleted")]
	public bool Deleted { get; set; }

	public override bool Equals(object obj)
	{
		var castObj = obj as BaseModel;

		if (castObj == null || string.IsNullOrEmpty(castObj.Id) || string.IsNullOrEmpty(Id))
			return false;

		return string.Equals(Id, castObj.Id, StringComparison.Ordinal);
	}

	public override int GetHashCode()
	{
		return Id.GetHashCode(StringComparison.Ordinal);
	}
}
