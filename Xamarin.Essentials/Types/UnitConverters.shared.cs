using System;

namespace Xamarin.Essentials
{
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

        const double meanEarthRadiusInKilometers = 6371.0;

        public static double FahrenheitToCelsius(double fahrenheit) =>
            (fahrenheit - 32.0) / 1.8;

        public static double CelsiusToFahrenheit(double celsius) =>
            (celsius * 1.8) + 32.0;

        public static double CelsiusToKelvin(double celsius) =>
           celsius + celsiusToKelvin;

        public static double KelvinToCelsius(double kelvin) =>
           kelvin - celsiusToKelvin;

        public static double MilesToMeters(double miles) =>
            miles * milesToMeters;

        public static double MilesToKilometers(double miles) =>
            miles * milesToKilometers;

        public static double KilometersToMiles(double kilometers) =>
            kilometers * kilometersToMiles;

        public static double DegreesToRadians(double degrees) =>
            degrees * degreesToRadians;

        public static double RadiansToDegrees(double radians) =>
            radians / degreesToRadians;

        public static double DegreesPerSecondToRadiansPerSecond(double degrees) =>
            HertzToRadiansPerSecond(DegreesPerSecondToHertz(degrees));

        public static double RadiansPerSecondToDegreesPerSecond(double radians) =>
            HertzToDegreesPerSecond(RadiansPerSecondToHertz(radians));

        public static double DegreesPerSecondToHertz(double degrees) =>
            degrees / totalDegrees;

        public static double RadiansPerSecondToHertz(double radians) =>
            radians / twoPi;

        public static double HertzToDegreesPerSecond(double hertz) =>
            hertz * totalDegrees;

        public static double HertzToRadiansPerSecond(double hertz) =>
            hertz * twoPi;

        public static double KilopascalsToHectopascals(double kpa) =>
            kpa * 10.0;

        public static double HectopascalsToKilopascals(double hpa) =>
            hpa / 10.0;

        public static double KilopascalsToPascals(double kpa) =>
            kpa * 1000.0;

        public static double HectopascalsToPascals(double hpa) =>
            hpa * 100.0;

        public static double AtmospheresToPascals(double atm) =>
            atm / atmospherePascals;

        public static double PascalsToAtmospheres(double pascals) =>
            pascals * atmospherePascals;

        public static double CoordinatesToMiles(double lat1, double lon1, double lat2, double lon2) =>
            KilometersToMiles(CoordinatesToKilometers(lat1, lon1, lat2, lon2));

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
    }
}
