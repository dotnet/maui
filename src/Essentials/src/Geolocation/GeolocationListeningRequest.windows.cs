#nullable enable

namespace Microsoft.Maui.Devices.Sensors
{
	public partial class GeolocationListeningRequest
	{
		internal uint PlatformDesiredAccuracy
		{
			get
			{
				return DesiredAccuracy.PlatformGetDesiredAccuracy();
			}
		}
	}
}
