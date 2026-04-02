using System.Text.Json;
using Maui.Controls.Sample.Models;

namespace Maui.Controls.Sample.Services;

public class WeatherService(HttpClient httpClient)
{
	public async Task<(string Icon, string Text)> GetWeatherForecastAsync(double latitude, double longitude, DateOnly date)
	{
		try
		{
			var url = $"https://api.open-meteo.com/v1/forecast?latitude={latitude.ToString("F4", System.Globalization.CultureInfo.InvariantCulture)}&longitude={longitude.ToString("F4", System.Globalization.CultureInfo.InvariantCulture)}&daily=temperature_2m_mean,weather_code&timezone=auto";

			var response = await httpClient.GetStringAsync(url);
			var forecast = JsonSerializer.Deserialize<WeatherForecast>(response);

			if (forecast?.Daily == null || forecast.Daily.Time.Count == 0)
			{
				return (FluentUI.weather_cloudy_24_regular, "Weather unavailable");
			}

			// Find the index for the requested date
			var dateString = date.ToString("yyyy-MM-dd");
			var index = forecast.Daily.Time.IndexOf(dateString);

			if (index < 0 || index >= forecast.Daily.TemperatureMean.Count)
			{
				return (FluentUI.weather_cloudy_24_regular, "Weather unavailable");
			}

			var temp = forecast.Daily.TemperatureMean[index];
			var weatherCode = forecast.Daily.WeatherCode[index];
			var icon = GetWeatherIcon(weatherCode);

			return (icon, $"{temp:F0}°C");
		}
		catch
		{
			return (FluentUI.weather_cloudy_24_regular, "Weather unavailable");
		}
	}

	static string GetWeatherIcon(int code)
	{
		return code switch
		{
			0 => FluentUI.weather_sunny_28_regular,
			1 or 2 => FluentUI.weather_partly_cloudy_day_24_regular,
			3 => FluentUI.weather_cloudy_24_regular,
			45 or 48 => FluentUI.weather_fog_24_regular,
			51 or 53 or 55 or 56 or 57 => FluentUI.weather_drizzle_24_regular,
			61 or 63 or 65 => FluentUI.weather_rain_24_regular,
			66 or 67 => FluentUI.weather_rain_24_regular,
			71 or 73 or 75 or 77 => FluentUI.weather_snowflake_24_regular,
			80 or 81 or 82 => FluentUI.weather_rain_showers_day_24_regular,
			85 or 86 => FluentUI.weather_snowflake_24_regular,
			95 => FluentUI.weather_thunderstorm_24_regular,
			96 or 99 => FluentUI.weather_thunderstorm_24_regular,
			_ => FluentUI.weather_cloudy_24_regular
		};
	}
}
