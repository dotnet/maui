using System;
using System.Net;
using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class PlacemarkExtensions
    {
        public static Task OpenMapsAsync(this Placemark placemark, MapLaunchOptions options) =>
            Map.OpenAsync(placemark, options);

        public static Task OpenMapsAsync(this Placemark placemark) =>
            Map.OpenAsync(placemark);

        internal static Placemark Escape(this Placemark placemark)
        {
            if (placemark == null)
                throw new ArgumentNullException(nameof(placemark));
            var escaped = new Placemark();

            if (placemark.Location == null)
                escaped.Location = new Location();
            else
                escaped.Location = new Location(placemark.Location);

            escaped.CountryCode = string.IsNullOrWhiteSpace(placemark.CountryCode) ? string.Empty : WebUtility.UrlEncode(placemark.CountryCode);
            escaped.CountryName = string.IsNullOrWhiteSpace(placemark.CountryName) ? string.Empty : WebUtility.UrlEncode(placemark.CountryName);
            escaped.FeatureName = string.IsNullOrWhiteSpace(placemark.FeatureName) ? string.Empty : WebUtility.UrlEncode(placemark.FeatureName);
            escaped.PostalCode = string.IsNullOrWhiteSpace(placemark.PostalCode) ? string.Empty : WebUtility.UrlEncode(placemark.PostalCode);
            escaped.Locality = string.IsNullOrWhiteSpace(placemark.Locality) ? string.Empty : WebUtility.UrlEncode(placemark.Locality);
            escaped.SubLocality = string.IsNullOrWhiteSpace(placemark.SubLocality) ? string.Empty : WebUtility.UrlEncode(placemark.SubLocality);
            escaped.Thoroughfare = string.IsNullOrWhiteSpace(placemark.Thoroughfare) ? string.Empty : WebUtility.UrlEncode(placemark.Thoroughfare);
            escaped.SubThoroughfare = string.IsNullOrWhiteSpace(placemark.SubThoroughfare) ? string.Empty : WebUtility.UrlEncode(placemark.SubThoroughfare);
            escaped.SubAdminArea = string.IsNullOrWhiteSpace(placemark.SubAdminArea) ? string.Empty : WebUtility.UrlEncode(placemark.SubAdminArea);
            escaped.AdminArea = string.IsNullOrWhiteSpace(placemark.AdminArea) ? string.Empty : WebUtility.UrlEncode(placemark.AdminArea);
            return escaped;
        }
    }
}
