using System;

using static System.Math;

namespace Xamarin.Essentials
{
    public enum DistanceUnits
    {
        Kilometers,
        Miles
    }

    public class Location
    {
        public Location()
        {
        }

        public Location(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
            TimestampUtc = DateTimeOffset.UtcNow;
        }

        public Location(double latitude, double longitude, DateTimeOffset timestamp)
        {
            Latitude = latitude;
            Longitude = longitude;
            TimestampUtc = timestamp;
        }

        public Location(Location point)
        {
            if (point == null)
                throw new ArgumentNullException(nameof(point));

            Latitude = point.Latitude;
            Longitude = point.Longitude;
            TimestampUtc = DateTimeOffset.UtcNow;
        }

        public DateTimeOffset TimestampUtc { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public double? Accuracy { get; set; }

        public static double CalculateDistance(Location locationStart, Location locationEnd, DistanceUnits units) =>
            CalculateDistance(locationStart.Latitude, locationEnd.Latitude, locationStart.Longitude, locationEnd.Longitude, units);

        public static double CalculateDistance(
            double latitudeStart,
            double latitudeEnd,
            double longitudeStart,
            double longitudeEnd,
            DistanceUnits units)
        {
            if (latitudeEnd == latitudeStart && longitudeEnd == longitudeStart)
                return 0;

            var rlat1 = PI * latitudeStart / 180.0;
            var rlat2 = PI * latitudeEnd / 180.0;
            var theta = longitudeStart - longitudeEnd;
            var rtheta = PI * theta / 180.0;
            var dist = (Sin(rlat1) * Sin(rlat2)) + (Cos(rlat1) * Cos(rlat2) * Cos(rtheta));
            dist = Acos(dist);
            dist = dist * 180.0 / PI;
            var final = dist * 60.0 * 1.1515;
            if (double.IsNaN(final) || double.IsInfinity(final) || double.IsNegativeInfinity(final) ||
                double.IsPositiveInfinity(final) || final < 0)
                return 0;

            if (units == DistanceUnits.Kilometers)
                return MilesToKilometers(final);

            return final;
        }

        public static double MilesToKilometers(double miles) => miles * 1.609344;

        public static double KilometersToMiles(double kilometers) => kilometers * .62137119;
    }
}
