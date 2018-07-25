using System;
using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class PlacemarkExtensions
    {
        public static Task OpenMapsAsync(this Placemark placemark, MapsLaunchOptions options) =>
            Maps.OpenAsync(placemark, options);

        public static Task OpenMapsAsync(this Placemark placemark) =>
            Maps.OpenAsync(placemark);

        internal static Placemark Escape(this Placemark placemark)
        {
            if (placemark == null)
                throw new ArgumentNullException(nameof(placemark));
            var escaped = new Placemark();

            if (placemark.Location == null)
                escaped.Location = new Location();
            else
                escaped.Location = new Location(placemark.Location);

            escaped.CountryCode = string.IsNullOrWhiteSpace(placemark.CountryCode) ? string.Empty : Uri.EscapeUriString(placemark.CountryCode);
            escaped.CountryName = string.IsNullOrWhiteSpace(placemark.CountryName) ? string.Empty : Uri.EscapeUriString(placemark.CountryName);
            escaped.FeatureName = string.IsNullOrWhiteSpace(placemark.FeatureName) ? string.Empty : Uri.EscapeUriString(placemark.FeatureName);
            escaped.PostalCode = string.IsNullOrWhiteSpace(placemark.PostalCode) ? string.Empty : Uri.EscapeUriString(placemark.PostalCode);
            escaped.Locality = string.IsNullOrWhiteSpace(placemark.Locality) ? string.Empty : Uri.EscapeUriString(placemark.Locality);
            escaped.SubLocality = string.IsNullOrWhiteSpace(placemark.SubLocality) ? string.Empty : Uri.EscapeUriString(placemark.SubLocality);
            escaped.Thoroughfare = string.IsNullOrWhiteSpace(placemark.Thoroughfare) ? string.Empty : Uri.EscapeUriString(placemark.Thoroughfare);
            escaped.SubThoroughfare = string.IsNullOrWhiteSpace(placemark.SubThoroughfare) ? string.Empty : Uri.EscapeUriString(placemark.SubThoroughfare);
            escaped.SubAdminArea = string.IsNullOrWhiteSpace(placemark.SubAdminArea) ? string.Empty : Uri.EscapeUriString(placemark.SubAdminArea);
            escaped.AdminArea = string.IsNullOrWhiteSpace(placemark.AdminArea) ? string.Empty : Uri.EscapeUriString(placemark.AdminArea);
            return escaped;
        }
    }
}
