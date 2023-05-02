#nullable enable
using System;

namespace Microsoft.Maui.Devices.Sensors
{
	/// <summary>
	/// Event args for the geolocation listening error event.
	/// </summary>
	public class GeolocationListeningFailedEventArgs : EventArgs
	{
		/// <summary>
		/// The geolocation error that describes the error that occurred.
		/// </summary>
		public GeolocationError Error { get; }

		/// <summary>
		/// Creates a new geolocation error event args object
		/// </summary>
		/// <param name="geolocationError">gelocation error to use for this object</param>
		public GeolocationListeningFailedEventArgs(GeolocationError geolocationError)
		{
			Error = geolocationError;
		}
	}
}