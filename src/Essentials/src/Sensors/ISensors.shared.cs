#nullable enable
using System;
using System.Numerics;
using Microsoft.Maui.Devices;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Devices.Sensors
{
	/// <summary>
	/// Base interface for all sensors.
	/// </summary>
	public interface ISensor : IDeviceCapabilities
	{
		/// <summary>
		/// Gets a value indicating whether the sensor is actively being monitored.
		/// </summary>
		bool IsMonitoring { get; }

		/// <summary>
		/// Start monitoring for changes to the sensor.
		/// </summary>
		/// <remarks>
		/// Will throw <see cref="FeatureNotSupportedException"/> if <see cref="IDeviceCapabilities.IsSupported"/> is <see langword="false"/>.
		/// Will throw <see cref="InvalidOperationException"/> if <see cref="IsMonitoring"/> is <see langword="true"/>.</remarks>
		/// <param name="sensorSpeed">Speed to monitor the sensor.</param>
		void Start(SensorSpeed sensorSpeed);

		/// <summary>
		/// Stop monitoring for changes to the sensor.
		/// </summary>
		void Stop();
	}
}