using System;
using System.Diagnostics;
using System.Threading.Tasks;
using CoreLocation;
using Foundation;
using MapKit;

namespace Xamarin.Essentials
{
    public static partial class Map
    {
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
            var address = new NSDictionary
            {
                [Contacts.CNPostalAddressKey.City] = new NSString(placemark.Locality),
                [Contacts.CNPostalAddressKey.Country] = new NSString(placemark.CountryName),
                [Contacts.CNPostalAddressKey.State] = new NSString(placemark.AdminArea),
                [Contacts.CNPostalAddressKey.Street] = new NSString(placemark.Thoroughfare),
                [Contacts.CNPostalAddressKey.PostalCode] = new NSString(placemark.PostalCode),
                [Contacts.CNPostalAddressKey.IsoCountryCode] = new NSString(placemark.CountryCode)
            };

            var coder = new CLGeocoder();
            CLPlacemark[] placemarks = null;
            try
            {
                placemarks = await coder.GeocodeAddressAsync(address);
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

        static Task OpenPlacemark(MKPlacemark placemark, MapLaunchOptions options)
        {
            var mapItem = new MKMapItem(placemark)
            {
                Name = options.Name ?? string.Empty
            };

            MKLaunchOptions launchOptions = null;
            if (options.NavigationMode != NavigationMode.None)
            {
                var mode = MKDirectionsMode.Default;

                switch (options.NavigationMode)
                {
                    case NavigationMode.Driving:
                        mode = MKDirectionsMode.Driving;
                        break;
                    case NavigationMode.Transit:
                        mode = MKDirectionsMode.Transit;
                        break;
                    case NavigationMode.Walking:
                        mode = MKDirectionsMode.Walking;
                        break;
                    case NavigationMode.Default:
                        mode = MKDirectionsMode.Default;
                        break;
                }
                launchOptions = new MKLaunchOptions
                {
                    DirectionsMode = mode
                };
            }

            var mapItems = new[] { mapItem };
            MKMapItem.OpenMaps(mapItems, launchOptions);
            return Task.CompletedTask;
        }
    }
}
