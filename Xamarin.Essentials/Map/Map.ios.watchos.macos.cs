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
#if __IOS__
            var address = new MKPlacemarkAddress
            {
                CountryCode = placemark.CountryCode,
                Country = placemark.CountryName,
                State = placemark.AdminArea,
                Street = placemark.Thoroughfare,
                City = placemark.Locality,
                Zip = placemark.PostalCode
            }.Dictionary;
#else
            var address = new NSMutableDictionary
            {
                [Contacts.CNPostalAddressKey.City] = new NSString(placemark.Locality ?? string.Empty),
                [Contacts.CNPostalAddressKey.Country] = new NSString(placemark.CountryName ?? string.Empty),
                [Contacts.CNPostalAddressKey.State] = new NSString(placemark.AdminArea ?? string.Empty),
                [Contacts.CNPostalAddressKey.Street] = new NSString(placemark.Thoroughfare ?? string.Empty),
                [Contacts.CNPostalAddressKey.PostalCode] = new NSString(placemark.PostalCode ?? string.Empty),
                [Contacts.CNPostalAddressKey.IsoCountryCode] = new NSString(placemark.CountryCode ?? string.Empty)
            };
#endif

            var coder = new CLGeocoder();
            CLPlacemark[] placemarks;
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
