#nullable enable
using System;

namespace Microsoft.Maui.Devices.Sensors
{
	/// <summary>
	/// Event arguments containing the current reading of <see cref="IGeolocation.LocationChanged"/>.
	/// </summary>
	public class GeolocationLocationChangedEventArgs : EventArgs
	{
		/// <summary>
		/// The current reading's location data.
		/// </summary>
		public Location Location { get; }

		/// <summary>
		/// Public constructor that takes in a reading for event arguments.
		/// </summary>
		/// <param name="location">The location data reading.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="location"/> is <see langword="null"/>.</exception>
		public GeolocationLocationChangedEventArgs(Location location)
		{
			if (location == null)
				throw new ArgumentNullException(nameof(location));

			Location = location;
		}
	}
}