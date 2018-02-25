using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using static System.Math;

namespace Microsoft.Caboodle
{
    public enum DistanceUnits
    {
        Kilometers,
        Miles
    }

    public class Position
    {
        public Position()
        {

        }

        public Position(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
			TimestampUtc = DateTimeOffset.UtcNow;
        }
        public Position(double latitude, double longitude, DateTimeOffset timestamp)
		{
			Latitude = latitude;
			Longitude = longitude;
			TimestampUtc = timestamp;
		}
        public Position(Position position)
        {
            if (position == null)
                throw new ArgumentNullException(nameof(position));

            Latitude = position.Latitude;
            Longitude = position.Longitude;
			TimestampUtc = DateTimeOffset.UtcNow;
        }

        public DateTimeOffset TimestampUtc { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public static double CalculateDistance(Position positionStart, Position positionEnd, DistanceUnits units = DistanceUnits.Miles) =>
			CalculateDistance(positionStart.Latitude, positionStart.Longitude, positionEnd.Latitude, positionEnd.Longitude, units);

        public static double CalculateDistance(
            double latitudeStart, double latitudeEnd,
            double longitudeStart, double longitudeEnd, DistanceUnits units = DistanceUnits.Miles)
        {
            if (latitudeEnd == latitudeStart && longitudeEnd == longitudeStart)
                return 0;

            var rlat1 = PI * latitudeStart / 180.0;
            var rlat2 = PI * latitudeEnd / 180.0;
            var theta = longitudeStart - longitudeEnd;
            var rtheta = PI * theta / 180.0;
            var dist = Sin(rlat1) * Sin(rlat2) + Cos(rlat1) * Cos(rlat2) * Cos(rtheta);
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
