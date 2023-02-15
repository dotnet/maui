namespace Microsoft.Maui.Devices.Sensors
{
	public partial class GeolocationRequest
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
