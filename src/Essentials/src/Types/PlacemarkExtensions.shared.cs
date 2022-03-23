using System;
using System.Threading.Tasks;

namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/PlacemarkExtensions.xml" path="Type[@FullName='Microsoft.Maui.Essentials.PlacemarkExtensions']/Docs" />
	public static partial class PlacemarkExtensions
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/PlacemarkExtensions.xml" path="//Member[@MemberName='OpenMapsAsync'][2]/Docs" />
		public static Task OpenMapsAsync(this Placemark placemark, MapLaunchOptions options) =>
			Map.OpenAsync(placemark, options);

		/// <include file="../../docs/Microsoft.Maui.Essentials/PlacemarkExtensions.xml" path="//Member[@MemberName='OpenMapsAsync'][1]/Docs" />
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
