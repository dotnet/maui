using System;
using System.Diagnostics;
using System.Threading.Tasks;
using CoreLocation;
using Foundation;
using MapKit;

namespace Xamarin.Essentials
{
    public static partial class Maps
    {
        internal static Task PlatformOpenMapsAsync(double latitude, double longitude, MapsLaunchOptions options)
        {
            if (string.IsNullOrWhiteSpace(options.Name))
                options.Name = string.Empty;

            NSDictionary dictionary = null;
            var placemark = new MKPlacemark(new CLLocationCoordinate2D(latitude, longitude), dictionary);
            return OpenPlacemark(placemark, options);
        }

        internal static async Task PlatformOpenMapsAsync(Placemark placemark, MapsLaunchOptions options)
        {
            var address = new MKPlacemarkAddress
            {
                CountryCode = placemark.CountryCode,
                Country = placemark.CountryName,
                State = placemark.AdminArea,
                Street = placemark.Thoroughfare,
                City = placemark.Locality
            };

            var coder = new CLGeocoder();
            CLPlacemark[] placemarks = null;
            try
            {
                placemarks = await coder.GeocodeAddressAsync(address.Dictionary);
            }
            catch
            {
                Debug.WriteLine("Unable to get geocode address from address");
                return;
            }

            if ((placemarks?.Length ?? 0) == 0)
            {
                Debug.WriteLine("No locations exist, please check address.");
            }

            await OpenPlacemark(new MKPlacemark(placemarks[0].Location.Coordinate, address), options);
        }

        static Task OpenPlacemark(MKPlacemark placemark, MapsLaunchOptions options)
        {
            var mapItem = new MKMapItem(placemark)
            {
                Name = options.Name ?? string.Empty
            };

            var mode = MKDirectionsMode.Default;

            switch (options.MapDirectionsMode)
            {
                case MapDirectionsMode.Driving:
                    mode = MKDirectionsMode.Driving;
                    break;
                case MapDirectionsMode.Transit:
                    mode = MKDirectionsMode.Transit;
                    break;
                case MapDirectionsMode.Walking:
                    mode = MKDirectionsMode.Walking;
                    break;
            }

            var launchOptions = new MKLaunchOptions
            {
                DirectionsMode = mode
            };

            var mapItems = new[] { mapItem };
            MKMapItem.OpenMaps(mapItems, launchOptions);
            return Task.CompletedTask;
        }
    }
}
