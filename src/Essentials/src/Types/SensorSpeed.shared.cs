namespace Microsoft.Maui.Devices.Sensors
{
	/// <summary>
	/// Represents the sensor speed to monitor device sensors for changes.
	/// </summary>
	public enum SensorSpeed
	{
		/// <summary>The device default sensor speed.</summary>
		Default = 0,

		/// <summary>Rate suitable for general user interface.</summary>
		UI = 1,

		/// <summary>Rate suitable for games.</summary>
		Game = 2,

		/// <summary>Get the sensor data as fast as possible.</summary>
		Fastest = 3,
	}

	internal static partial class SensorSpeedExtensions
	{
		// Timing intervals to match Android sensor speeds in milliseconds
		// https://developer.android.com/guide/topics/sensors/sensors_overview
		internal const uint sensorIntervalDefault = 200;
		internal const uint sensorIntervalUI = 60;
		internal const uint sensorIntervalGame = 20;
		internal const uint sensorIntervalFastest = 5;
	}
}
