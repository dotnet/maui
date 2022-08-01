using Microsoft.Maui.Devices.Sensors;

namespace Microsoft.Maui.Controls.Maps
{
	public class MapClickedEventArgs
	{
		public Location Position { get; }

		public MapClickedEventArgs(Location position)
		{
			Position = position;
		}
	}
}
