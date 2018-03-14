using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Caboodle
{
    public static partial class Geocoding
    {
        public static string MapKey { get; set; }

        public static Task<IEnumerable<Placemark>> GetPlacemarksAsync(Location location)
        {
            if (location == null)
                throw new ArgumentNullException(nameof(location));

            return GetPlacemarksAsync(location.Latitude, location.Longitude);
        }
    }
}
