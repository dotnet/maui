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
}
