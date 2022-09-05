namespace Microsoft.Maui.Devices.Sensors
{
	public partial class ListeningRequest
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
