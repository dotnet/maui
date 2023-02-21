#nullable enable
using System;

namespace Microsoft.Maui.Devices.Sensors
{
	/// <summary>
	/// Represents levels of accuracy when determining the device location.
	/// </summary>
	public enum GeolocationAccuracy
	{
		// Default is Medium
		/// <summary>Represents default accuracy (Medium), typically within 30-500 meters.</summary>
		Default = 0,

		// iOS:     ThreeKilometers         (3000m)
		// Android: ACCURACY_LOW, POWER_LOW (500m)
		// UWP:     3000                    (1000-5000m)
		/// <summary>Represents the lowest accuracy, using the least power to obtain and typically within 1000-5000 meters.</summary>
		Lowest = 1,

		// iOS:     Kilometer               (1000m)
		// Android: ACCURACY_LOW, POWER_MED (500m)
		// UWP:     1000                    (300-3000m)
		/// <summary>Represents low accuracy, typically within 300-3000 meters.</summary>
		Low = 2,

		// iOS:     HundredMeters           (100m)
		// Android: ACCURACY_MED, POWER_MED (100-500m)
		// UWP:     100                     (30-500m)
		/// <summary>Represents medium accuracy, typically within 30-500 meters.</summary>
		Medium = 3,

		// iOS:     NearestTenMeters        (10m)
		// Android: ACCURACY_HI, POWER_HI   (0-100m)
		// UWP:     High                    (<=10m)
		/// <summary>Represents high accuracy, typically within 10-100 meters.</summary>
		High = 4,

		// iOS:     Best                    (0m)
		// Android: ACCURACY_HI, POWER_HI   (0-100m)
		// UWP:     High                    (<=10m)
		/// <summary>Represents the best accuracy, using the most power to obtain and typically within 10 meters.</summary>
		Best = 5
	}

	/// <summary>
	/// Represents the criteria for a location request.
	/// </summary>
	public partial class GeolocationRequest
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="GeolocationRequest"/> class with default options.
		/// </summary>
		public GeolocationRequest()
		{
			Timeout = TimeSpan.Zero;
			DesiredAccuracy = GeolocationAccuracy.Default;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GeolocationRequest"/> class with the specified accuracy.
		/// </summary>
		/// <param name="accuracy">The desired accuracy for determining the location.</param>
		public GeolocationRequest(GeolocationAccuracy accuracy)
		{
			Timeout = TimeSpan.Zero;
			DesiredAccuracy = accuracy;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GeolocationRequest"/> class with the specified accuracy and timeout.
		/// </summary>
		/// <param name="accuracy">The desired accuracy for determining the location.</param>
		/// <param name="timeout">A timeout value after which the location determination will be cancelled.</param>
		public GeolocationRequest(GeolocationAccuracy accuracy, TimeSpan timeout)
		{
			Timeout = timeout;
			DesiredAccuracy = accuracy;
		}

		/// <summary>
		/// Gets or sets the location request timeout.
		/// </summary>
		public TimeSpan Timeout { get; set; }

		/// <summary>
		/// Gets or sets the desired accuracy of the resulting location.
		/// </summary>
		public GeolocationAccuracy DesiredAccuracy { get; set; }

		/// <summary>
		/// Gets or sets whether to request full permission to temporarily use location services with full accuracy.
		/// </summary>
		/// <remarks>This value is only used on iOS 14+. Using this functionality requires the <c>NSLocationTemporaryUsageDescriptionDictionary</c> key to be present in the <c>info.plist</c> file.</remarks>
		public bool RequestFullAccuracy { get; set; }

		/// <summary>
		/// Returns a string representation of the current values of <see cref="GeolocationRequest"/>.
		/// </summary>
		/// <returns>A string representation of this instance in the format of <c>DesiredAccuracy: {value}, Timeout: {value}</c>.</returns>
		public override string ToString() =>
			$"{nameof(DesiredAccuracy)}: {DesiredAccuracy}, {nameof(Timeout)}: {Timeout}";
	}
}
