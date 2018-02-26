using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Caboodle
{
	public partial class Geocoding
    {
        public static string MapKey { get; set; }

        public static Task<IEnumerable<Location>> GetLocationsAsync(GeoPoint point)
        {
            if (point == null)
                throw new ArgumentNullException(nameof(point));

            return GetLocationsAsync(point.Latitude, point.Longitude);
        }
    }
}
