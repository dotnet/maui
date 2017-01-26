using CoreLocation;
using Foundation;

namespace Xamarin.Forms.Maps.MacOS.Extra
{
	delegate void CLGeocodeCompletionHandler(CLPlacemark[] placemarks, NSError error);

	[BaseType(typeof(NSObject))]
	interface CLGeocoder
	{
		[Export("isGeocoding")]
		bool Geocoding { get; }

		[Export("reverseGeocodeLocation:completionHandler:")]
		[Async]
		void ReverseGeocodeLocation(CLLocation location, CLGeocodeCompletionHandler completionHandler);

		[Export("geocodeAddressDictionary:completionHandler:")]
		[Async]
		void GeocodeAddress(NSDictionary addressDictionary, CLGeocodeCompletionHandler completionHandler);

		[Export("geocodeAddressString:completionHandler:")]
		[Async]
		void GeocodeAddress(string addressString, CLGeocodeCompletionHandler completionHandler);

		[Export("geocodeAddressString:inRegion:completionHandler:")]
		[Async]
		void GeocodeAddress(string addressString, CLRegion region, CLGeocodeCompletionHandler completionHandler);

		[Export("cancelGeocode")]
		void CancelGeocode();
	}
}
