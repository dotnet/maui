namespace MauiApp._1.Data;

public class WeatherForecastService
{
	private static readonly string[] Summaries = new[]
	{
		"Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
	};

	public Task<WeatherForecast[]> GetForecastAsync(DateTime startDate) =>
		Task.FromResult(Enumerable.Range(1, 5).Select(index => new WeatherForecast(
				startDate.AddDays(index),
				Random.Shared.Next(-20, 55),
				Summaries[Random.Shared.Next(Summaries.Length)])).ToArray());
}

