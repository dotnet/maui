using System.Threading.Tasks;
using Microsoft.Maui.Devices.Sensors;

namespace Microsoft.Maui.ApplicationModel
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/Map.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Map']/Docs" />
	public class MapImplementation : IMap
	{
		public Task OpenMapsAsync(double latitude, double longitude, MapLaunchOptions options)
			=> throw ExceptionUtils.NotSupportedOrImplementedException;

		public Task OpenMapsAsync(Placemark placemark, MapLaunchOptions options)
			=> throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
