using System.Globalization;
using PoolMath.Data;
using PoolMathApp.Helpers;
using PoolMathApp.Models;

namespace PoolMathApp.Xaml
{
	public class LogToOneLineTimeAndWeatherConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is not Log log)
				return null;

			var ft = new FormattedText();

			if (log.LogTimestamp.Year == DateTime.UtcNow.Year)
				ft.Add(new TextSpan(log.LogTimestamp.ToLocalTime().ToString("MMM d, h:mm tt")));
			else
				ft.Add(new TextSpan(log.LogTimestamp.ToLocalTime().ToString("MMM d yyyy, h:mm tt")));

			if (log.Weather is null)
				return ft.ToFormattedString();

			ft.Add(new TextSpan("  ∙  "));

			var units = PoolMath.Units.US;

			if (log.Weather.Temperature.HasValue)
			{
				var degSymbol = units == PoolMath.Units.US ? "F" : "C";

				var degVal = log.Weather.Temperature.Value;

				ft.Add("\ue9a9", fontFamily: "Weather");
				ft.Add($" {degVal.ToString("0")}° {degSymbol}   ", fontWeight: Models.FontWeight.Normal);
			}

			//if (!string.IsNullOrEmpty(Weather.UvIndexDescription))
			//	txt += "UV: " + Weather.UvIndexDescription + bullet;

			if (log.Weather.WindSpeed.HasValue)
			{
				var speedSymbol = units == PoolMath.Units.US ? " mph" : " kph";

				var speedVal = log.Weather.Temperature.Value;

				ft.Add("\ue9c4", fontFamily: "Weather");

				ft.Add($" {Math.Abs(speedVal).ToString("0")}{speedSymbol}", fontWeight: Models.FontWeight.Normal);
			}

			return ft.ToFormattedString();
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
