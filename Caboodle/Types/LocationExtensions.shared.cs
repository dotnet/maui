using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Caboodle.Types
{
    public static partial class LocationExtensions
    {
        public static double CalculateDistance(this Location locationStart, Location locationEnd, DistanceUnits units) =>
            Location.CalculateDistance(locationStart, locationEnd, units);
    }
}
