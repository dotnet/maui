using System.Threading.Tasks;

namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/LocationExtensions.xml" path="Type[@FullName='Microsoft.Maui.Essentials.LocationExtensions']/Docs" />
	public static partial class LocationExtensions
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/LocationExtensions.xml" path="//Member[@MemberName='CalculateDistance' and position()=0]/Docs" />
		public static double CalculateDistance(this Location locationStart, double latitudeEnd, double longitudeEnd, DistanceUnits units) =>
			Location.CalculateDistance(locationStart, latitudeEnd, longitudeEnd, units);

		/// <include file="../../docs/Microsoft.Maui.Essentials/LocationExtensions.xml" path="//Member[@MemberName='CalculateDistance' and position()=1]/Docs" />
		public static double CalculateDistance(this Location locationStart, Location locationEnd, DistanceUnits units) =>
			Location.CalculateDistance(locationStart, locationEnd, units);

		/// <include file="../../docs/Microsoft.Maui.Essentials/LocationExtensions.xml" path="//Member[@MemberName='OpenMapsAsync' and position()=0]/Docs" />
		public static Task OpenMapsAsync(this Location location, MapLaunchOptions options) =>
			Map.OpenAsync(location, options);

		/// <include file="../../docs/Microsoft.Maui.Essentials/LocationExtensions.xml" path="//Member[@MemberName='OpenMapsAsync' and position()=1]/Docs" />
		public static Task OpenMapsAsync(this Location location) =>
			Map.OpenAsync(location);
	}
}
