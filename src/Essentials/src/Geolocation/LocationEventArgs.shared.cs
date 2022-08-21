#nullable enable
using System;

namespace Microsoft.Maui.Devices.Sensors
{
	public class LocationEventArgs : EventArgs
	{
		public Location Location { get; }

		public LocationEventArgs(Location location)
		{
			if (location == null)
				throw new ArgumentNullException(nameof(location));

			Location = location;
		}
	}
}