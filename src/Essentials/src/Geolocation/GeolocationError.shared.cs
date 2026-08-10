#nullable enable

namespace Microsoft.Maui.Devices.Sensors
{
	/// <summary>
	/// Error values for listening for geolocation changes
	/// </summary>
	public enum GeolocationError
	{
		/// <summary>
		/// The provider was unable to retrieve any position data.
		/// On Android this is sent when no location provider is available that satisfies the requested geolocation accuracy.
		/// On iOS this means getting location data has failed.
		/// On Windows this means no location data is available from any source.
		/// </summary>
		PositionUnavailable,

		/// <summary>
		/// The app is not, or no longer, authorized to receive location data.
		/// On Android this is not used.
		/// On iOS this means authorization for getting locations has changed.
		/// On Windows this means location sources are turned off.
		/// </summary>
		Unauthorized,
	}
}