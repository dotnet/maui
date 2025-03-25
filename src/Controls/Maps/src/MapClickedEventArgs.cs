using Microsoft.Maui.Devices.Sensors;

namespace Microsoft.Maui.Controls.Maps
{
	/// <summary>
	/// Event arguments that are associated with a click/tap event that occurs on the map control.
	/// </summary>
	public class MapClickedEventArgs
	{
		/// <summary>
		/// Gets the location of the click/tap.
		/// </summary>
		public Location Location { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="MapClickedEventArgs"/> class with a location.
		/// </summary>
		/// <param name="location">The location data associated with this event.</param>
		public MapClickedEventArgs(Location location)
		{
			Location = location;
		}
	}
}
