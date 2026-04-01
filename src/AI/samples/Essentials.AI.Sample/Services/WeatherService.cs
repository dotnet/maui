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
			var icon = WeatherCodeExtensions.GetWeatherIcon(weatherCode);

			return (icon, $"{temp:F0}°C");
		}
		catch
		{
			return (FluentUI.weather_cloudy_24_regular, "Weather unavailable");
		}
	}
}
