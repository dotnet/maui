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
		/// </summary>
		/// <remarks>
		/// Android: Sent when no location provider is available that satisfies the requested geolocation accuracy.
		/// iOS: Getting location data has failed.
		/// Windows: No location data is available from any source.
		/// </remarks>
		PositionUnavailable,

		/// <summary>
		/// The app is not, or no longer, authorized to receive location data.
		/// </summary>
		/// <remarks>
		/// Android: Not used.
		/// iOS: Authorization for getting locations has changed.
		/// Windows: Location sources are turned off.
		/// </remarks>
		Unauthorized,
	}
}