using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Caboodle
{
	public static partial class PositionExtensions
	{
        public static double CalculateDistance(this Position positionStart, Position positionEnd, DistanceUnits units = DistanceUnits.Miles) =>
            Position.CalculateDistance(positionStart, positionEnd, units);

    }
}
