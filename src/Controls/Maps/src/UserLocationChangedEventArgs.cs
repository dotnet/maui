using Microsoft.Maui.Devices.Sensors;

namespace Microsoft.Maui.Controls.Maps
{
	/// <summary>
	/// Event arguments for the <see cref="Map.UserLocationChanged"/> event.
	/// </summary>
	public class UserLocationChangedEventArgs : System.EventArgs
	{
		/// <summary>
		/// Gets the updated user location.
		/// </summary>
		public Location Location { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="UserLocationChangedEventArgs"/> class.
		/// </summary>
		/// <param name="location">The user's updated location.</param>
		public UserLocationChangedEventArgs(Location location)
		{
			Location = location;
		}
	}
}
