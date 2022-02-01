using System;

namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/UnitConverters.xml" path="Type[@FullName='Microsoft.Maui.Essentials.UnitConverters']/Docs" />
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

		/// <include file="../../docs/Microsoft.Maui.Essentials/UnitConverters.xml" path="//Member[@MemberName='FahrenheitToCelsius']/Docs" />
		public static double FahrenheitToCelsius(double fahrenheit) =>
			(fahrenheit - 32.0) / 1.8;

		/// <include file="../../docs/Microsoft.Maui.Essentials/UnitConverters.xml" path="//Member[@MemberName='CelsiusToFahrenheit']/Docs" />
		public static double CelsiusToFahrenheit(double celsius) =>
			(celsius * 1.8) + 32.0;

		/// <include file="../../docs/Microsoft.Maui.Essentials/UnitConverters.xml" path="//Member[@MemberName='CelsiusToKelvin']/Docs" />
		public static double CelsiusToKelvin(double celsius) =>
		   celsius + celsiusToKelvin;

		/// <include file="../../docs/Microsoft.Maui.Essentials/UnitConverters.xml" path="//Member[@MemberName='KelvinToCelsius']/Docs" />
		public static double KelvinToCelsius(double kelvin) =>
		   kelvin - celsiusToKelvin;

		/// <include file="../../docs/Microsoft.Maui.Essentials/UnitConverters.xml" path="//Member[@MemberName='MilesToMeters']/Docs" />
		public static double MilesToMeters(double miles) =>
			miles * milesToMeters;

		/// <include file="../../docs/Microsoft.Maui.Essentials/UnitConverters.xml" path="//Member[@MemberName='MilesToKilometers']/Docs" />
		public static double MilesToKilometers(double miles) =>
			miles * milesToKilometers;

		/// <include file="../../docs/Microsoft.Maui.Essentials/UnitConverters.xml" path="//Member[@MemberName='KilometersToMiles']/Docs" />
		public static double KilometersToMiles(double kilometers) =>
			kilometers * kilometersToMiles;

		/// <include file="../../docs/Microsoft.Maui.Essentials/UnitConverters.xml" path="//Member[@MemberName='DegreesToRadians']/Docs" />
		public static double DegreesToRadians(double degrees) =>
			degrees * degreesToRadians;

		/// <include file="../../docs/Microsoft.Maui.Essentials/UnitConverters.xml" path="//Member[@MemberName='RadiansToDegrees']/Docs" />
		public static double RadiansToDegrees(double radians) =>
			radians / degreesToRadians;

		/// <include file="../../docs/Microsoft.Maui.Essentials/UnitConverters.xml" path="//Member[@MemberName='PoundsToKilograms']/Docs" />
		public static double PoundsToKilograms(double pounds) =>
			pounds * poundsToKg;

		/// <include file="../../docs/Microsoft.Maui.Essentials/UnitConverters.xml" path="//Member[@MemberName='PoundsToStones']/Docs" />
		public static double PoundsToStones(double pounds) =>
			pounds * poundsToStones;

		/// <include file="../../docs/Microsoft.Maui.Essentials/UnitConverters.xml" path="//Member[@MemberName='StonesToPounds']/Docs" />
		public static double StonesToPounds(double stones) =>
		   stones * stonesToPounds;

		/// <include file="../../docs/Microsoft.Maui.Essentials/UnitConverters.xml" path="//Member[@MemberName='KilogramsToPounds']/Docs" />
		public static double KilogramsToPounds(double kilograms) =>
			kilograms * kgToPounds;

		/// <include file="../../docs/Microsoft.Maui.Essentials/UnitConverters.xml" path="//Member[@MemberName='DegreesPerSecondToRadiansPerSecond']/Docs" />
		public static double DegreesPerSecondToRadiansPerSecond(double degrees) =>
			HertzToRadiansPerSecond(DegreesPerSecondToHertz(degrees));

		/// <include file="../../docs/Microsoft.Maui.Essentials/UnitConverters.xml" path="//Member[@MemberName='RadiansPerSecondToDegreesPerSecond']/Docs" />
		public static double RadiansPerSecondToDegreesPerSecond(double radians) =>
			HertzToDegreesPerSecond(RadiansPerSecondToHertz(radians));

		/// <include file="../../docs/Microsoft.Maui.Essentials/UnitConverters.xml" path="//Member[@MemberName='DegreesPerSecondToHertz']/Docs" />
		public static double DegreesPerSecondToHertz(double degrees) =>
			degrees / totalDegrees;

		/// <include file="../../docs/Microsoft.Maui.Essentials/UnitConverters.xml" path="//Member[@MemberName='RadiansPerSecondToHertz']/Docs" />
		public static double RadiansPerSecondToHertz(double radians) =>
			radians / twoPi;

		/// <include file="../../docs/Microsoft.Maui.Essentials/UnitConverters.xml" path="//Member[@MemberName='HertzToDegreesPerSecond']/Docs" />
		public static double HertzToDegreesPerSecond(double hertz) =>
			hertz * totalDegrees;

		/// <include file="../../docs/Microsoft.Maui.Essentials/UnitConverters.xml" path="//Member[@MemberName='HertzToRadiansPerSecond']/Docs" />
		public static double HertzToRadiansPerSecond(double hertz) =>
			hertz * twoPi;

		/// <include file="../../docs/Microsoft.Maui.Essentials/UnitConverters.xml" path="//Member[@MemberName='KilopascalsToHectopascals']/Docs" />
		public static double KilopascalsToHectopascals(double kpa) =>
			kpa * 10.0;

		/// <include file="../../docs/Microsoft.Maui.Essentials/UnitConverters.xml" path="//Member[@MemberName='HectopascalsToKilopascals']/Docs" />
		public static double HectopascalsToKilopascals(double hpa) =>
			hpa / 10.0;

		/// <include file="../../docs/Microsoft.Maui.Essentials/UnitConverters.xml" path="//Member[@MemberName='KilopascalsToPascals']/Docs" />
		public static double KilopascalsToPascals(double kpa) =>
			kpa * 1000.0;

		/// <include file="../../docs/Microsoft.Maui.Essentials/UnitConverters.xml" path="//Member[@MemberName='HectopascalsToPascals']/Docs" />
		public static double HectopascalsToPascals(double hpa) =>
			hpa * 100.0;

		/// <include file="../../docs/Microsoft.Maui.Essentials/UnitConverters.xml" path="//Member[@MemberName='AtmospheresToPascals']/Docs" />
		public static double AtmospheresToPascals(double atm) =>
			atm * atmospherePascals;

		/// <include file="../../docs/Microsoft.Maui.Essentials/UnitConverters.xml" path="//Member[@MemberName='PascalsToAtmospheres']/Docs" />
		public static double PascalsToAtmospheres(double pascals) =>
			pascals / atmospherePascals;

		/// <include file="../../docs/Microsoft.Maui.Essentials/UnitConverters.xml" path="//Member[@MemberName='CoordinatesToMiles']/Docs" />
		public static double CoordinatesToMiles(double lat1, double lon1, double lat2, double lon2) =>
			KilometersToMiles(CoordinatesToKilometers(lat1, lon1, lat2, lon2));

		/// <include file="../../docs/Microsoft.Maui.Essentials/UnitConverters.xml" path="//Member[@MemberName='CoordinatesToKilometers']/Docs" />
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

			var a = dLat2 + (dLon2 * Math.Cos(lat1) * Math.Cos(lat2));
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
