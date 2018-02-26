using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Caboodle
{
	public partial class Geocoding
    {
        public static Task<IEnumerable<Location>> GetLocationsAsync(double latitude, double longitude) =>
            throw new NotImplentedInReferenceAssembly();


        public static Task<IEnumerable<GeoPoint>> GetPointsAsync(string address) =>
            throw new NotImplentedInReferenceAssembly();
    }
}
