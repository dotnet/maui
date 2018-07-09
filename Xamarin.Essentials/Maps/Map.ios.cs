using System.Threading.Tasks;
using CoreLocation;
using Foundation;
using MapKit;

namespace Xamarin.Essentials
{
    public static partial class Map
    {
        internal static Task PlatformOpenMapsAsync(Location location, MapLaunchOptions options)
        => PlatformOpenMapsAsync(location.Latitude, location.Longitude, options);

        internal static Task PlatformOpenMapsAsync(double latitude, double longitude, MapLaunchOptions options)
        {
            if (string.IsNullOrWhiteSpace(options.Name))
                options.Name = string.Empty;

            NSDictionary dictionary = null;
            var placemark = new MKPlacemark(new CLLocationCoordinate2D(latitude, longitude), dictionary);
            return OpenPlacemark(placemark, options);
        }

        internal static async Task PlatformOpenMapsAsync(Placemark placemark, MapLaunchOptions options)
        {
            var adress = new MKPlacemarkAddress
            {
                CountryCode = placemark.CountryCode,
                Country = placemark.CountryName,
                State = placemark.AdminArea,
                Street = placemark.Thoroughfare,
                City = placemark.Locality
            };
            var coder = new CLGeocoder();
            var placemarks = await coder.GeocodeAddressAsync(adress.Dictionary).ConfigureAwait(false);
            var mkPlacemark = new MKPlacemark(placemarks[0].Location.Coordinate, adress);
            await OpenPlacemark(mkPlacemark, options);
        }

        static Task OpenPlacemark(MKPlacemark placemark, MapLaunchOptions options)
        {
            var mapItem = new MKMapItem(placemark)
            {
                Name = options.Name
            };

            MKLaunchOptions launchOptions = null;
            if (options.MapNavigation != MapNavigation.Default)
            {
                launchOptions = new MKLaunchOptions
                {
                    DirectionsMode = options.MapNavigation == MapNavigation.Driving ? MKDirectionsMode.Driving : MKDirectionsMode.Walking
                };
            }

            var mapItems = new[] { mapItem };
            MKMapItem.OpenMaps(mapItems, launchOptions);
            return Task.CompletedTask;
        }
    }
}
