using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Caboodle
{
	public partial class Geocoding
    {
        public static Task<IEnumerable<Address>> GetAddressesAsync(double latitude, double longitude) =>
            throw new NotImplentedInReferenceAssembly();


        public static Task<IEnumerable<Position>> GetPositionsAsync(string address) =>
            throw new NotImplentedInReferenceAssembly();
    }
}
