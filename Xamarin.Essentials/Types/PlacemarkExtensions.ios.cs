using System.Collections.Generic;
using System.Linq;
using CoreLocation;

namespace Xamarin.Essentials
{
    public static partial class PlacemarkExtensions
    {
        internal static IEnumerable<Placemark> ToPlacemarks(this IEnumerable<CLPlacemark> addresses)
        {
            return addresses.Select(address => new Placemark
            {
                Location = address.ToLocation(),
                FeatureName = address.Name,
                PostalCode = address.PostalCode,
                SubLocality = address.SubLocality,
                CountryCode = address.IsoCountryCode,
                CountryName = address.Country,
                Thoroughfare = address.Thoroughfare,
                SubThoroughfare = address.SubThoroughfare,
                Locality = address.Locality,
                AdminArea = address.AdministrativeArea,
                SubAdminArea = address.SubAdministrativeArea
            });
        }
    }
}
