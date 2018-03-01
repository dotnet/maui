using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Caboodle
{
    public partial class Geocoding
    {
        public static Task<IEnumerable<Placemark>> GetPlacemarksAsync(double latitude, double longitude) =>
            throw new NotImplentedInReferenceAssemblyException();

        public static Task<IEnumerable<Location>> GetLocationsAsync(string address) =>
            throw new NotImplentedInReferenceAssemblyException();
    }
}
