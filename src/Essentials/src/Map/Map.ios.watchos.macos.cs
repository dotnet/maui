using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Contacts;
using CoreLocation;
using Foundation;
using MapKit;

namespace Microsoft.Maui.Essentials.Implementations
{
	public class MapImplementation : IMap
	{
		public Task OpenMapsAsync(double latitude, double longitude, MapLaunchOptions options)
		{
			if (string.IsNullOrWhiteSpace(options.Name))
				options.Name = string.Empty;

			NSDictionary dictionary = null;
			var placemark = new MKPlacemark(new CLLocationCoordinate2D(latitude, longitude), dictionary);
			return OpenPlacemark(placemark, options);
		}

		public async Task OpenMapsAsync(Placemark placemark, MapLaunchOptions options)
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
				[CNPostalAddressKey.City] = new NSString(placemark.Locality ?? string.Empty),
				[CNPostalAddressKey.Country] = new NSString(placemark.CountryName ?? string.Empty),
				[CNPostalAddressKey.State] = new NSString(placemark.AdminArea ?? string.Empty),
				[CNPostalAddressKey.Street] = new NSString(placemark.Thoroughfare ?? string.Empty),
				[CNPostalAddressKey.PostalCode] = new NSString(placemark.PostalCode ?? string.Empty),
				[CNPostalAddressKey.IsoCountryCode] = new NSString(placemark.CountryCode ?? string.Empty)
			};
#endif

			var resolvedPlacemarks = await GetPlacemarksAsync(address);
			if (resolvedPlacemarks?.Length > 0)
			{
				await OpenPlacemark(new MKPlacemark(resolvedPlacemarks[0].Location.Coordinate, address), options);
			}
			else
			{
#if __IOS__ || __MACOS__
				// https://developer.apple.com/library/archive/featuredarticles/iPhoneURLScheme_Reference/MapLinks/MapLinks.html
				var uri = $"http://maps.apple.com/?q={placemark.GetEscapedAddress()}";
				var nsurl = NSUrl.FromString(uri);

				await Launcher.OpenAsync(nsurl);
#else
				await OpenPlacemark(new MKPlacemark(default, address), options);
#endif
			}
		}

		static async Task<CLPlacemark[]> GetPlacemarksAsync(NSDictionary address)
		{
			using var geocoder = new CLGeocoder();

			try
			{
				System.Diagnostics.Debug.Assert(!OperatingSystem.IsIOSVersionAtLeast(11));
				// we need to await to keep the geocoder alive until after the async
				return await geocoder.GeocodeAddressAsync(address);
			}
			catch
			{
				Debug.WriteLine("Unable to get geocode address from address");
				return null;
			}
		}

		static Task<bool> OpenPlacemark(MKPlacemark placemark, MapLaunchOptions options)
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
			return Task.FromResult(MKMapItem.OpenMaps(mapItems, launchOptions));
		}
	}
}
