#nullable enable
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Devices.Sensors;

namespace Microsoft.Maui.Devices.Sensors
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/Geolocation.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Geolocation']/Docs" />
	public static partial class Geolocation
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/Geolocation.xml" path="//Member[@MemberName='GetLastKnownLocationAsync']/Docs" />
		public static Task<Location?> GetLastKnownLocationAsync() =>
			Current.GetLastKnownLocationAsync();

		/// <include file="../../docs/Microsoft.Maui.Essentials/Geolocation.xml" path="//Member[@MemberName='GetLocationAsync'][1]/Docs" />
		public static Task<Location?> GetLocationAsync() =>
			Current.GetLocationAsync();

		/// <include file="../../docs/Microsoft.Maui.Essentials/Geolocation.xml" path="//Member[@MemberName='GetLocationAsync'][2]/Docs" />
		public static Task<Location?> GetLocationAsync(GeolocationRequest request) =>
			Current.GetLocationAsync(request);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Geolocation.xml" path="//Member[@MemberName='GetLocationAsync'][3]/Docs" />
		public static Task<Location?> GetLocationAsync(GeolocationRequest request, CancellationToken cancelToken) =>
			Current.GetLocationAsync(request, cancelToken);

		static IGeolocation Current => Devices.Sensors.Geolocation.Default;
	}
}
