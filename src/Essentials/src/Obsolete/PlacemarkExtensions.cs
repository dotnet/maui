#nullable enable
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices.Sensors;

namespace Microsoft.Maui.ApplicationModel
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
	}
}
