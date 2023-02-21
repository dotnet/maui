using Microsoft.Maui.Devices.Sensors;

namespace Microsoft.Maui.Controls.Maps
{
	public class MapClickedEventArgs
	{
		public Location Location { get; }

		public MapClickedEventArgs(Location location)
		{
			Location = location;
		}
	}
}
