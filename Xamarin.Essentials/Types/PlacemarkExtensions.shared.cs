using System;
using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class PlacemarkExtensions
    {
        public static Task OpenMapsAsync(this Placemark placemark, MapLaunchOptions options) =>
            Map.OpenAsync(placemark, options);

        public static Task OpenMapsAsync(this Placemark placemark) =>
            Map.OpenAsync(placemark);

        internal static string GetEscapedAddress(this Placemark placemark)
        {
            if (placemark == null)
                throw new ArgumentNullException(nameof(placemark));

            var address = $"{placemark.Thoroughfare} {placemark.Locality} {placemark.AdminArea} {placemark.PostalCode} {placemark.CountryName}";

            return Uri.EscapeDataString(address);
        }
    }
}
