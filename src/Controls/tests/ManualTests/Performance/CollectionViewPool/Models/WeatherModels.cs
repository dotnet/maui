using System;
using Newtonsoft.Json;

namespace PoolMath.Data;

public class WeatherLocation
{
	[JsonProperty("id")]
	[System.Text.Json.Serialization.JsonPropertyName("id")]
	public string Id { get; set; }

	[JsonProperty("zip")]
	[System.Text.Json.Serialization.JsonPropertyName("zip")]
	public string Zip { get; set; }

	[JsonProperty("name")]
	[System.Text.Json.Serialization.JsonPropertyName("name")]
	public string Name { get; set; }
}

public class WeatherSummary
{
	[JsonProperty("timestamp")]
	[System.Text.Json.Serialization.JsonPropertyName("timestamp")]
	public DateTime Timestamp { get; set; }

	[JsonProperty("desc")]
	[System.Text.Json.Serialization.JsonPropertyName("desc")]
	public string Description { get; set; }

	[JsonProperty("icon")]
	[System.Text.Json.Serialization.JsonPropertyName("icon")]
	public long? Icon { get; set; }

	[JsonProperty("temp")]
	[System.Text.Json.Serialization.JsonPropertyName("temp")]
	public double? Temperature { get; set; }

	[JsonProperty("tempFeelsLike")]
	[System.Text.Json.Serialization.JsonPropertyName("tempFeelsLike")]
	public double? TemperatureFeelsLike { get; set; }

	[JsonProperty("relHumidity")]
	[System.Text.Json.Serialization.JsonPropertyName("relHumidity")]
	public long? RelativeHumidity { get; set; }

	[JsonProperty("windDir")]
	[System.Text.Json.Serialization.JsonPropertyName("windDir")]
	public long? WindDirectionDegrees { get; set; }

	[JsonProperty("windDirDesc")]
	[System.Text.Json.Serialization.JsonPropertyName("windDirDesc")]
	public string WindDirectionDescription { get; set; }

	[JsonProperty("windSpeed")]
	[System.Text.Json.Serialization.JsonPropertyName("windSpeed")]
	public double? WindSpeed { get; set; }

	[JsonProperty("windGustSpeed")]
	[System.Text.Json.Serialization.JsonPropertyName("windGustSpeed")]
	public double? WindGustSpeed { get; set; }

	[JsonProperty("uvIndex")]
	[System.Text.Json.Serialization.JsonPropertyName("uvIndex")]
	public long? UvIndex { get; set; }

	[JsonProperty("uvIndexDesc")]
	[System.Text.Json.Serialization.JsonPropertyName("uvIndexDesc")]
	public string UvIndexDescription { get; set; }

	[JsonProperty("visibility")]
	[System.Text.Json.Serialization.JsonPropertyName("visibility")]
	public double? Visibility { get; set; }

	[JsonProperty("cloudCover")]
	[System.Text.Json.Serialization.JsonPropertyName("cloudCover")]
	public long? CloudCover { get; set; }

	[JsonProperty("pressure")]
	[System.Text.Json.Serialization.JsonPropertyName("pressure")]
	public double? Pressure { get; set; }

	[JsonProperty("precipPast24Hours")]
	[System.Text.Json.Serialization.JsonPropertyName("precipPast24Hours")]
	public double? PrecipitationPast24Hours { get; set; }
}

public class WeatherLog : Log
{
	public const string API_ROUTE = "weatherlogs";

	public const string TYPE_NAME = "weatherlog";

	[JsonIgnore]
	[System.Text.Json.Serialization.JsonIgnore]
	public override string ApiRoute => API_ROUTE;

	[JsonProperty("type")]
	[System.Text.Json.Serialization.JsonPropertyName("type")]
	public override string Type => TYPE_NAME;

	[JsonProperty("desc")]
	[System.Text.Json.Serialization.JsonPropertyName("desc")]
	public string Description { get; set; }

	[JsonProperty("icon")]
	[System.Text.Json.Serialization.JsonPropertyName("icon")]
	public long? Icon { get; set; }

	[JsonProperty("isDay")]
	[System.Text.Json.Serialization.JsonPropertyName("isDay")]
	public bool? IsDayTime { get; set; }

	[JsonProperty("temp")]
	[System.Text.Json.Serialization.JsonPropertyName("temp")]
	public double? Temperature { get; set; }

	[JsonProperty("tempFeelsLike")]
	[System.Text.Json.Serialization.JsonPropertyName("tempFeelsLike")]
	public double? TemperatureFeelsLike { get; set; }

	[JsonProperty("tempFeelsLikeShade")]
	[System.Text.Json.Serialization.JsonPropertyName("tempFeelsLikeShade")]
	public double? TemperatureFeelsLikeShade { get; set; }

	[JsonProperty("relHumidity")]
	[System.Text.Json.Serialization.JsonPropertyName("relHumidity")]
	public long? RelativeHumidity { get; set; }

	[JsonProperty("dewPoint")]
	[System.Text.Json.Serialization.JsonPropertyName("dewPoint")]
	public double? DewPoint { get; set; }

	[JsonProperty("windDir")]
	[System.Text.Json.Serialization.JsonPropertyName("windDir")]
	public long? WindDirectionDegrees { get; set; }

	[JsonProperty("windDirDesc")]
	[System.Text.Json.Serialization.JsonPropertyName("windDirDesc")]
	public string WindDirectionDescription { get; set; }

	[JsonProperty("windSpeed")]
	[System.Text.Json.Serialization.JsonPropertyName("windSpeed")]
	public double? WindSpeed { get; set; }


	[JsonProperty("windGustSpeed")]
	[System.Text.Json.Serialization.JsonPropertyName("windGustSpeed")]
	public double? WindGustSpeed { get; set; }

	[JsonProperty("uvIndex")]
	[System.Text.Json.Serialization.JsonPropertyName("uvIndex")]
	public long? UvIndex { get; set; }

	[JsonProperty("uvIndexDesc")]
	[System.Text.Json.Serialization.JsonPropertyName("uvIndexDesc")]
	public string UvIndexDescription { get; set; }

	[JsonProperty("visibility")]
	[System.Text.Json.Serialization.JsonPropertyName("visibility")]
	public double? Visibility { get; set; }

	[JsonProperty("visibilityObstructions")]
	[System.Text.Json.Serialization.JsonPropertyName("visibilityObstructions")]
	public string VisibilityObstructions { get; set; }

	[JsonProperty("cloudCover")]
	[System.Text.Json.Serialization.JsonPropertyName("cloudCover")]
	public long? CloudCover { get; set; }

	[JsonProperty("ceiling")]
	[System.Text.Json.Serialization.JsonPropertyName("ceiling")]
	public double? Ceiling { get; set; }

	[JsonProperty("pressure")]
	[System.Text.Json.Serialization.JsonPropertyName("pressure")]
	public double? Pressure { get; set; }

	[JsonProperty("pressureTendency")]
	[System.Text.Json.Serialization.JsonPropertyName("pressureTendency")]
	public string PressureTendency { get; set; }

	[JsonProperty("past24HrTempDeparture")]
	[System.Text.Json.Serialization.JsonPropertyName("past24HrTempDeparture")]
	public double? Past24HourTemperatureDeparture { get; set; }

	[JsonProperty("apparentTemp")]
	[System.Text.Json.Serialization.JsonPropertyName("apparentTemp")]
	public double? ApparentTemperature { get; set; }

	[JsonProperty("windChillTemp")]
	[System.Text.Json.Serialization.JsonPropertyName("windChillTemp")]
	public double? WindChillTemperature { get; set; }

	[JsonProperty("wetBulbTemp")]
	[System.Text.Json.Serialization.JsonPropertyName("wetBulbTemp")]
	public double? WetBulbTemperature { get; set; }

	[JsonProperty("precip1hr")]
	[System.Text.Json.Serialization.JsonPropertyName("precip1hr")]
	public double? Precip1Hr { get; set; }

	[JsonProperty("precip")]
	[System.Text.Json.Serialization.JsonPropertyName("precip")]
	public double? Precipitation { get; set; }

	[JsonProperty("precipPastHour")]
	[System.Text.Json.Serialization.JsonPropertyName("precipPastHour")]
	public double? PrecipitationPastHour { get; set; }

	[JsonProperty("precipPast3Hours")]
	[System.Text.Json.Serialization.JsonPropertyName("precipPast3Hours")]
	public double? PrecipitationPast3Hours { get; set; }

	[JsonProperty("precipPast6Hours")]
	[System.Text.Json.Serialization.JsonPropertyName("precipPast6Hours")]
	public double? PrecipitationPast6Hours { get; set; }

	[JsonProperty("precipPast9Hours")]
	[System.Text.Json.Serialization.JsonPropertyName("precipPast9Hours")]
	public double? PrecipitationPast9Hours { get; set; }

	[JsonProperty("precipPast12Hours")]
	[System.Text.Json.Serialization.JsonPropertyName("precipPast12Hours")]
	public double? PrecipitationPast12Hours { get; set; }

	[JsonProperty("precipPast18Hours")]
	[System.Text.Json.Serialization.JsonPropertyName("precipPast18Hours")]
	public double? PrecipitationPast18Hours { get; set; }

	[JsonProperty("precipPast24Hours")]
	[System.Text.Json.Serialization.JsonPropertyName("precipPast24Hours")]
	public double? PrecipitationPast24Hours { get; set; }


	[JsonProperty("tempPast6HoursMin")]
	[System.Text.Json.Serialization.JsonPropertyName("tempPast6HoursMin")]
	public double? TemperaturePast6HoursMin { get; set; }

	[JsonProperty("tempPast6HoursMax")]
	[System.Text.Json.Serialization.JsonPropertyName("tempPast6HoursMax")]
	public double? TemperaturePast6HoursMax { get; set; }

	[JsonProperty("tempPast12HoursMin")]
	[System.Text.Json.Serialization.JsonPropertyName("tempPast12HoursMin")]
	public double? TemperaturePast12HoursMin { get; set; }

	[JsonProperty("tempPast12HoursMax")]
	[System.Text.Json.Serialization.JsonPropertyName("tempPast12HoursMax")]
	public double? TemperaturePast12HoursMax { get; set; }

	[JsonProperty("tempPast24HoursMin")]
	[System.Text.Json.Serialization.JsonPropertyName("tempPast24HoursMin")]
	public double? TemperaturePast24HoursMin { get; set; }

	[JsonProperty("tempPast24HoursMax")]
	[System.Text.Json.Serialization.JsonPropertyName("tempPast24HoursMax")]
	public double? TemperaturePast24HoursMax { get; set; }

}

public static class WeatherModelExtensions
{
	public static WeatherSummary GetSummary(this WeatherLog weatherLog)
	{
		return new WeatherSummary
		{
			Timestamp = weatherLog.LogTimestamp,
			CloudCover = weatherLog.CloudCover,
			Description = weatherLog.Description,
			Icon = weatherLog.Icon,
			PrecipitationPast24Hours = weatherLog.PrecipitationPast24Hours,
			Pressure = weatherLog.Pressure,
			RelativeHumidity = weatherLog.RelativeHumidity,
			Temperature = weatherLog.Temperature,
			TemperatureFeelsLike = weatherLog.TemperatureFeelsLike,
			UvIndex = weatherLog.UvIndex,
			UvIndexDescription = weatherLog.UvIndexDescription,
			Visibility = weatherLog.Visibility,
			WindSpeed = weatherLog.WindSpeed,
			WindGustSpeed = weatherLog.WindGustSpeed,
			WindDirectionDegrees = weatherLog.WindDirectionDegrees,
			WindDirectionDescription = weatherLog.WindDirectionDescription,
		};
	}
}
