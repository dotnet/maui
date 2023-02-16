using System;

namespace Microsoft.Maui.Media
{
	/// <summary>
	/// Static class with built-in unit converters.
	/// </summary>
	public static class UnitConverters
	{
		const double twoPi = 2.0 * Math.PI;
		const double totalDegrees = 360.0;
		const double atmospherePascals = 101325.0;
		const double degreesToRadians = Math.PI / 180.0;
		const double milesToKilometers = 1.609344;
		const double milesToMeters = 1609.344;
		const double kilometersToMiles = 1.0 / milesToKilometers;
		const double celsiusToKelvin = 273.15;
		const double poundsToKg = 0.45359237;
		const double poundsToStones = 0.07142857;
		const double stonesToPounds = 14;
		const double kgToPounds = 2.204623;

		const double meanEarthRadiusInKilometers = 6371.0;

		const double internationalFootDefinition = 0.3048;
		const double usSurveyFootDefinition = 1200.0 / 3937;

		/// <summary>
		/// Converts temperatures from Fahrenheit to Celsius.
		/// </summary>
		/// <param name="fahrenheit">The value in Fahrenheit to convert.</param>
		/// <returns>The value from <paramref name="fahrenheit"/> in degrees Celsius.</returns>
		public static double FahrenheitToCelsius(double fahrenheit) =>
			(fahrenheit - 32.0) / 1.8;

		/// <summary>
		/// Converts temperatures from Celsius to Fahrenheit.
		/// </summary>
		/// <param name="celsius">The value in Celcius to convert.</param>
		/// <returns>The value from <paramref name="celsius"/> in degrees Fahrenheit.</returns>
		public static double CelsiusToFahrenheit(double celsius) =>
			celsius * 1.8 + 32.0;

		/// <summary>
		/// Converts temperatures from Celsius to Kelvin.
		/// </summary>
		/// <param name="celsius">The value in Celcius to convert.</param>
		/// <returns>The value from <paramref name="celsius"/> in degrees Kelvin.</returns>
		public static double CelsiusToKelvin(double celsius) =>
			celsius + celsiusToKelvin;

		/// <summary>
		/// Converts temperatures from Kelvin to Celsius.
		/// </summary>
		/// <param name="kelvin">The value in Kelvin to convert.</param>
		/// <returns>The value from <paramref name="kelvin"/> in degrees Celcius.</returns>
		public static double KelvinToCelsius(double kelvin) =>
			kelvin - celsiusToKelvin;

		/// <summary>
		/// Converts distances from miles to meters.
		/// </summary>
		/// <param name="miles">The value in miles to convert.</param>
		/// <returns>The value from <paramref name="miles"/> in meters.</returns>
		public static double MilesToMeters(double miles) =>
			miles * milesToMeters;

		/// <summary>
		/// Converts distances from miles to kilometers.
		/// </summary>
		/// <param name="miles">The value in miles to convert.</param>
		/// <returns>The value from <paramref name="miles"/> in kilometers.</returns>
		public static double MilesToKilometers(double miles) =>
			miles * milesToKilometers;

		/// <summary>
		/// Converts distances from kilometers to miles.
		/// </summary>
		/// <param name="kilometers">The value in kilometers to convert.</param>
		/// <returns>The value from <paramref name="kilometers"/> in miles.</returns>
		public static double KilometersToMiles(double kilometers) =>
			kilometers * kilometersToMiles;

		/// <summary>
		/// Converts degrees to radian.
		/// </summary>
		/// <param name="degrees">The value in degrees to convert.</param>
		/// <returns>The value from <paramref name="degrees"/> in radian.</returns>
		public static double DegreesToRadians(double degrees) =>
			degrees * degreesToRadians;

		/// <summary>
		/// Converts radians to degrees.
		/// </summary>
		/// <param name="radians">The value in radians to convert.</param>
		/// <returns>The value from <paramref name="radians"/> in degrees.</returns>
		public static double RadiansToDegrees(double radians) =>
			radians / degreesToRadians;

		/// <summary>
		/// Converts pounds to kilograms.
		/// </summary>
		/// <param name="pounds">The value in pounds to convert.</param>
		/// <returns>The value from <paramref name="pounds"/> in kilograms.</returns>
		public static double PoundsToKilograms(double pounds) =>
			pounds * poundsToKg;

		/// <summary>
		/// Converts pounds to stones.
		/// </summary>
		/// <param name="pounds">The value in pounds to convert.</param>
		/// <returns>The value from <paramref name="pounds"/> in stones.</returns>
		public static double PoundsToStones(double pounds) =>
			pounds * poundsToStones;

		/// <summary>
		/// Converts stones to pounds.
		/// </summary>
		/// <param name="stones">The value in stones to convert.</param>
		/// <returns>The value from <paramref name="stones"/> in pounds.</returns>
		public static double StonesToPounds(double stones) =>
			stones * stonesToPounds;

		/// <summary>
		/// Converts kilograms to pounds.
		/// </summary>
		/// <param name="kilograms">The value in kilograms to convert.</param>
		/// <returns>The value from <paramref name="kilograms"/> in pounds.</returns>
		public static double KilogramsToPounds(double kilograms) =>
			kilograms * kgToPounds;

		/// <summary>
		/// Converts degrees per second to radians per second.
		/// </summary>
		/// <param name="degrees">The value in degrees per second to convert.</param>
		/// <returns>The value from <paramref name="degrees"/> in radians per second.</returns>
		public static double DegreesPerSecondToRadiansPerSecond(double degrees) =>
			HertzToRadiansPerSecond(DegreesPerSecondToHertz(degrees));

		/// <summary>
		/// Converts radians per second to degrees per second.
		/// </summary>
		/// <param name="radians">The value in radians per second to convert.</param>
		/// <returns>The value from <paramref name="radians"/> in degrees per second.</returns>
		public static double RadiansPerSecondToDegreesPerSecond(double radians) =>
			HertzToDegreesPerSecond(RadiansPerSecondToHertz(radians));

		/// <summary>
		/// Converts degrees per second to hertz.
		/// </summary>
		/// <param name="degrees">The value in degrees per second to convert.</param>
		/// <returns>The value from <paramref name="degrees"/> in hertz.</returns>
		public static double DegreesPerSecondToHertz(double degrees) =>
			degrees / totalDegrees;

		/// <summary>
		/// Converts radians per second to hertz.
		/// </summary>
		/// <param name="radians">The value in radians per second to convert.</param>
		/// <returns>The value from <paramref name="radians"/> in hertz.</returns>
		public static double RadiansPerSecondToHertz(double radians) =>
			radians / twoPi;

		/// <summary>
		/// Converts hertz to degrees per second.
		/// </summary>
		/// <param name="hertz">The value in degrees per second to convert.</param>
		/// <returns>The value from <paramref name="hertz"/> in degrees per second.</returns>
		public static double HertzToDegreesPerSecond(double hertz) =>
			hertz * totalDegrees;

		/// <summary>
		/// Converts hertz to radians per second.
		/// </summary>
		/// <param name="hertz">The value in radians per second to convert.</param>
		/// <returns>The value from <paramref name="hertz"/> in radians per second.</returns>
		public static double HertzToRadiansPerSecond(double hertz) =>
			hertz * twoPi;

		/// <summary>
		/// Converts Kilopascals to Hectopascals.
		/// </summary>
		/// <param name="kpa">The value in Kilopascals convert.</param>
		/// <returns>The value from <paramref name="kpa"/> in Hectopascals.</returns>
		public static double KilopascalsToHectopascals(double kpa) =>
			kpa * 10.0;

		/// <summary>
		/// Converts Kilopascals to Hectopascals.
		/// </summary>
		/// <param name="hpa">The value in Hectopascals convert.</param>
		/// <returns>The value from <paramref name="hpa"/> in Kilopascals.</returns>
		public static double HectopascalsToKilopascals(double hpa) =>
			hpa / 10.0;

		/// <summary>
		/// Converts Kilopascals to Pascals.
		/// </summary>
		/// <param name="kpa">The value in Kilopascals convert.</param>
		/// <returns>The value from <paramref name="kpa"/> in Pascals.</returns>
		public static double KilopascalsToPascals(double kpa) =>
			kpa * 1000.0;

		/// <summary>
		/// Converts Hectopascals to Pascals.
		/// </summary>
		/// <param name="hpa">The value in Hectopascals convert.</param>
		/// <returns>The value from <paramref name="hpa"/> in Pascals.</returns>
		public static double HectopascalsToPascals(double hpa) =>
			hpa * 100.0;

		/// <summary>
		/// Converts Atmospheres to Pascals.
		/// </summary>
		/// <param name="atm">The value in Atmospheres convert.</param>
		/// <returns>The value from <paramref name="atm"/> in Pascals.</returns>
		public static double AtmospheresToPascals(double atm) =>
			atm * atmospherePascals;

		/// <summary>
		/// Converts Pascals to Atmospheres.
		/// </summary>
		/// <param name="pascals">The value in Pascals convert.</param>
		/// <returns>The value from <paramref name="pascals"/> in Atmospheres.</returns>
		public static double PascalsToAtmospheres(double pascals) =>
			pascals / atmospherePascals;

		/// <summary>
		/// Calculates the distance between two coordinates in miles.
		/// </summary>
		/// <param name="lat1">First latitude.</param>
		/// <param name="lon1">First longitude.</param>
		/// <param name="lat2">Second latitude.</param>
		/// <param name="lon2">Second longitude.</param>
		/// <returns>The distance in miles.</returns>
		public static double CoordinatesToMiles(double lat1, double lon1, double lat2, double lon2) =>
			KilometersToMiles(CoordinatesToKilometers(lat1, lon1, lat2, lon2));

		/// <summary>
		/// Calculates the distance between two coordinates in kilometers.
		/// </summary>
		/// <param name="lat1">First latitude.</param>
		/// <param name="lon1">First longitude.</param>
		/// <param name="lat2">Second latitude.</param>
		/// <param name="lon2">Second longitude.</param>
		/// <returns>The distance in kilometers.</returns>
		public static double CoordinatesToKilometers(double lat1, double lon1, double lat2, double lon2)
		{
			if (lat1 == lat2 && lon1 == lon2)
				return 0;

			var dLat = DegreesToRadians(lat2 - lat1);
			var dLon = DegreesToRadians(lon2 - lon1);

			lat1 = DegreesToRadians(lat1);
			lat2 = DegreesToRadians(lat2);

			var dLat2 = Math.Sin(dLat / 2) * Math.Sin(dLat / 2);
			var dLon2 = Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

			var a = dLat2 + dLon2 * Math.Cos(lat1) * Math.Cos(lat2);
			var c = 2 * Math.Asin(Math.Sqrt(a));

			return meanEarthRadiusInKilometers * c;
		}

		/// <summary>
		/// International survey foot defined as exactly 0.3048 meters by convention in 1959. This is the most common modern foot measure.
		/// </summary>
		public static double MetersToInternationalFeet(double meters) =>
			meters / internationalFootDefinition;

		/// <summary>
		/// International survey foot defined as exactly 0.3048 meters by convention in 1959. This is the most common modern foot measure.
		/// </summary>
		public static double InternationalFeetToMeters(double internationalFeet) =>
			internationalFeet * internationalFootDefinition;

		/// <summary>
		/// Exactly 1200/3937 meters by definition. In decimal terms approximately 0.304 800 609 601 219 meters. Variation from the common international foot of exactly 0.3048 meters may only be considerable over large survey distances.
		/// </summary>
		public static double MetersToUSSurveyFeet(double meters) =>
			meters / usSurveyFootDefinition;

		/// <summary>
		/// Exactly 1200/3937 meters by definition. In decimal terms approximately 0.304 800 609 601 219 meters. Variation from the common international foot of exactly 0.3048 meters may only be considerable over large survey distances.
		/// </summary>
		public static double USSurveyFeetToMeters(double usFeet) =>
			usFeet * usSurveyFootDefinition;
	}
}
