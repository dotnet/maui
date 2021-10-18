namespace MauiApp._1.Data;

public record WeatherForecast(DateTime Date, int TemperatureC, string Summary)
{
	public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

