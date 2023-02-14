#nullable enable

namespace Microsoft.Maui.Devices.Sensors
{
	public partial class GeolocationRequest
	{
		internal double PlatformDesiredAccuracy
		{
			get
			{
				return DesiredAccuracy.PlatformDesiredAccuracy();
			}
		}
	}
}
