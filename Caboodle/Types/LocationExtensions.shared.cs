using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Caboodle
{
	public static partial class PositionExtensions
	{
        public static double CalculateDistance(this GeoPoint positionStart, GeoPoint positionEnd, DistanceUnits units = DistanceUnits.Miles) =>
			GeoPoint.CalculateDistance(positionStart, positionEnd, units);

    }
}
