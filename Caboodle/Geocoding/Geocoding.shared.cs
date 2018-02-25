using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Caboodle
{
	public partial class Geocoding
    {
        public static string MapKey { get; set; }

        public static Task<IEnumerable<Address>> GetAddressesAsync(Position position)
        {
            if (position == null)
                throw new ArgumentNullException(nameof(position));

            return GetAddressesAsync(position.Latitude, position.Longitude);
        }
    }
}
