#nullable enable
using System;

namespace Microsoft.Maui.Devices.Sensors
{
	public class GeolocationLocationChangedEventArgs : EventArgs
	{
		public Location Location { get; }

		public GeolocationLocationChangedEventArgs(Location location)
		{
			if (location == null)
				throw new ArgumentNullException(nameof(location));

			Location = location;
		}
	}
}