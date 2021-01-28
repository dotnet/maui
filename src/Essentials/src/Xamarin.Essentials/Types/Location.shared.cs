using System;

using static System.Math;

namespace Xamarin.Essentials
{
    public enum DistanceUnits
    {
        Kilometers,
        Miles
    }

    public enum AltitudeReferenceSystem
    {
        Unspecified = 0,
        Terrain = 1,
        Ellipsoid = 2,
        Geoid = 3,
        Surface = 4
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
            Timestamp = DateTimeOffset.UtcNow;
        }

        public Location(double latitude, double longitude, DateTimeOffset timestamp)
        {
            Latitude = latitude;
            Longitude = longitude;
            Timestamp = timestamp;
        }

        public Location(double latitude, double longitude, double altitude)
        {
            Latitude = latitude;
            Longitude = longitude;
            Altitude = altitude;
            Timestamp = DateTimeOffset.UtcNow;
        }

        public Location(Location point)
        {
            if (point == null)
                throw new ArgumentNullException(nameof(point));

            Latitude = point.Latitude;
            Longitude = point.Longitude;
            Timestamp = DateTime.UtcNow;
            Altitude = point.Altitude;
            Accuracy = point.Accuracy;
            VerticalAccuracy = point.VerticalAccuracy;
            Speed = point.Speed;
            Course = point.Course;
            IsFromMockProvider = point.IsFromMockProvider;
        }

        public DateTimeOffset Timestamp { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public double? Altitude { get; set; }

        public double? Accuracy { get; set; }

        public double? VerticalAccuracy { get; set; }

        public double? Speed { get; set; }

        public double? Course { get; set; }

        public bool IsFromMockProvider { get; set; }

        public AltitudeReferenceSystem AltitudeReferenceSystem { get; set; }

        public static double CalculateDistance(double latitudeStart, double longitudeStart, Location locationEnd, DistanceUnits units) =>
            CalculateDistance(latitudeStart, longitudeStart, locationEnd.Latitude, locationEnd.Longitude, units);

        public static double CalculateDistance(Location locationStart, double latitudeEnd, double longitudeEnd, DistanceUnits units) =>
           CalculateDistance(locationStart.Latitude, locationStart.Longitude, latitudeEnd, longitudeEnd, units);

        public static double CalculateDistance(Location locationStart, Location locationEnd, DistanceUnits units) =>
            CalculateDistance(locationStart.Latitude, locationStart.Longitude, locationEnd.Latitude, locationEnd.Longitude, units);

        public static double CalculateDistance(
            double latitudeStart,
            double longitudeStart,
            double latitudeEnd,
            double longitudeEnd,
            DistanceUnits units)
        {
            switch (units)
            {
                case DistanceUnits.Kilometers:
                    return UnitConverters.CoordinatesToKilometers(latitudeStart, longitudeStart, latitudeEnd, longitudeEnd);
                case DistanceUnits.Miles:
                    return UnitConverters.CoordinatesToMiles(latitudeStart, longitudeStart, latitudeEnd, longitudeEnd);
                default:
                    throw new ArgumentOutOfRangeException(nameof(units));
            }
        }

        public override string ToString() =>
            $"{nameof(Latitude)}: {Latitude}, " +
            $"{nameof(Longitude)}: {Longitude}, " +
            $"{nameof(Altitude)}: {Altitude}, " +
            $"{nameof(Accuracy)}: {Accuracy}, " +
            $"{nameof(VerticalAccuracy)}: {VerticalAccuracy}, " +
            $"{nameof(Speed)}: {Speed}, " +
            $"{nameof(Course)}: {Course}, " +
            $"{nameof(Timestamp)}: {Timestamp}";
    }
}
