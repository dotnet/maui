using System.Text.Json;
using Maui.Controls.Sample.Models;

namespace Maui.Controls.Sample.Services;

public class WeatherService(HttpClient httpClient)
{
	public async Task<string> GetWeatherForecastAsync(double latitude, double longitude, DateOnly date)
    {
        try
        {
            var url = $"https://api.open-meteo.com/v1/forecast?latitude={latitude:F4}&longitude={longitude:F4}&daily=temperature_2m_mean,weather_code&timezone=auto";
            
            var response = await httpClient.GetStringAsync(url);
            var forecast = JsonSerializer.Deserialize<WeatherForecast>(response);

            if (forecast?.Daily == null || forecast.Daily.Time.Count == 0)
            {
                return "☁️ Weather unavailable";
            }

            // Find the index for the requested date
            var dateString = date.ToString("yyyy-MM-dd");
            var index = forecast.Daily.Time.IndexOf(dateString);

            if (index < 0 || index >= forecast.Daily.TemperatureMean.Count)
            {
                return "☁️ Weather unavailable";
            }

            var temp = forecast.Daily.TemperatureMean[index];
            var weatherCode = forecast.Daily.WeatherCode[index];
            var emoji = WeatherCodeExtensions.GetWeatherEmoji(weatherCode);

            return $"{emoji} {temp:F0}°C";
        }
        catch
        {
            return "☁️ Weather unavailable";
        }
    }
}
