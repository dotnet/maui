namespace MauiApp._1.Data;

public class WeatherForecastService
{
	private static readonly string[] Summaries = new[]
	{
		"Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
	};

	public Task<WeatherForecast[]> GetForecastAsync(DateTime startDate)
	{
		var forecasts = Enumerable.Range(1, 5).Select(index =>
			new WeatherForecast
			(
				DateTime.Now.AddDays(index),
				Random.Shared.Next(-20, 55),
				summaries[Random.Shared.Next(summaries.Length)]
			))
		   .ToArray();
		return forecasts;
	}
}

