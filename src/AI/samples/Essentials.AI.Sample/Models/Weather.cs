using System.Text.Json.Serialization;

namespace Maui.Controls.Sample.Models;

public record WeatherForecast
{
    [JsonPropertyName("latitude")]
    public double Latitude { get; init; }

    [JsonPropertyName("longitude")]
    public double Longitude { get; init; }

    [JsonPropertyName("daily")]
    public DailyWeather Daily { get; init; } = new();
}

public record DailyWeather
{
    [JsonPropertyName("time")]
    public List<string> Time { get; init; } = [];

    [JsonPropertyName("temperature_2m_mean")]
    public List<double> TemperatureMean { get; init; } = [];

    [JsonPropertyName("weather_code")]
    public List<int> WeatherCode { get; init; } = [];
}

public static class WeatherCodeExtensions
{
    public static string GetWeatherEmoji(int code)
    {
        return code switch
        {
            0 => "☀️",
            1 or 2 => "🌤️",
            3 => "☁️",
            45 or 48 => "🌫️",
            51 or 53 or 55 or 56 or 57 => "🌧️",
            61 or 63 or 65 => "🌧️",
            66 or 67 => "🌧️",
            71 or 73 or 75 or 77 => "❄️",
            80 or 81 or 82 => "🌧️",
            85 or 86 => "❄️",
            95 => "⛈️",
            96 or 99 => "⛈️",
            _ => "☁️"
        };
    }

    public static string GetWeatherDescription(int code)
    {
        return code switch
        {
            0 => "Clear sky",
            1 => "Mainly clear",
            2 => "Partly cloudy",
            3 => "Overcast",
            45 => "Fog",
            48 => "Depositing rime fog",
            51 => "Light drizzle",
            53 => "Moderate drizzle",
            55 => "Dense drizzle",
            56 => "Light freezing drizzle",
            57 => "Dense freezing drizzle",
            61 => "Slight rain",
            63 => "Moderate rain",
            65 => "Heavy rain",
            66 => "Light freezing rain",
            67 => "Heavy freezing rain",
            71 => "Slight snow",
            73 => "Moderate snow",
            75 => "Heavy snow",
            77 => "Snow grains",
            80 => "Slight rain showers",
            81 => "Moderate rain showers",
            82 => "Violent rain showers",
            85 => "Slight snow showers",
            86 => "Heavy snow showers",
            95 => "Thunderstorm",
            96 => "Thunderstorm with slight hail",
            99 => "Thunderstorm with heavy hail",
            _ => "Unknown"
        };
    }
}
