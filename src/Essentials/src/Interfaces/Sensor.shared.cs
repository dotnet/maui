#nullable enable

namespace Microsoft.Maui.Devices.Sensors
{
	/// <summary>
	/// Represents a sensor that can be monitored for changes.
	/// </summary>
	public interface ISensor : Microsoft.Maui.Devices.IDeviceCapability
	{
		/// <summary>
		/// Gets a value indicating whether the sensor is actively being monitored.
		/// </summary>
		bool IsMonitoring { get; }

		/// <summary>
		/// Start monitoring for changes to the sensor.
		/// </summary>
		/// <param name="sensorSpeed">The speed to monitor for changes.</param>
		void Start(SensorSpeed sensorSpeed);

		/// <summary>
		/// Stop monitoring for changes to the sensor.
		/// </summary>
		void Stop();
	}
}