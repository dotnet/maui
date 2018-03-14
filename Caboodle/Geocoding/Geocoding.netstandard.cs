using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Caboodle
{
    public static partial class Geocoding
    {
        public static Task<IEnumerable<Placemark>> GetPlacemarksAsync(double latitude, double longitude) =>
            throw new NotImplentedInReferenceAssembly();

        public static Task<IEnumerable<Location>> GetLocationsAsync(string address) =>
            throw new NotImplentedInReferenceAssembly();
    }
}
