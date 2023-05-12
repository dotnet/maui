#nullable enable
using System;

namespace Microsoft.Maui.Devices.Sensors
{
	/// <summary>
	/// Request options for listening to geolocations
	/// </summary>
	public partial class GeolocationListeningRequest
	{
		/// <summary>
		/// Creates a new request object with default values
		/// </summary>
		public GeolocationListeningRequest()
			: this(GeolocationAccuracy.Default)
		{
		}

		/// <summary>
		/// Creates a new request object with given accuracy.
		/// </summary>
		/// <param name="accuracy">The desired geolocation accuracy.</param>
		public GeolocationListeningRequest(GeolocationAccuracy accuracy)
			: this(accuracy, TimeSpan.FromSeconds(1))
		{
		}

		/// <summary>
		/// Creates a new request object with given accuracy and minimum time.
		/// </summary>
		/// <param name="accuracy">The desired geolocation accuracy.</param>
		/// <param name="minimumTime">The minimum time between location updates being sent.</param>
		public GeolocationListeningRequest(GeolocationAccuracy accuracy, TimeSpan minimumTime)
		{
			DesiredAccuracy = accuracy;
			MinimumTime = minimumTime;
		}

		/// <summary>
		/// Minimum time between location updates being sent. This value must positive. Most location
		/// sensors may not return locations in intervals shorter than 1 second.
		/// </summary>
		public TimeSpan MinimumTime { get; set; } = TimeSpan.FromSeconds(1);

		/// <summary>
		/// The desired minimum accuracy for the location updates being sent. Locations that don't
		/// satisfy this accuracy are not sent using the event handler.
		/// </summary>
		public GeolocationAccuracy DesiredAccuracy { get; set; } = GeolocationAccuracy.Default;
	}
}
