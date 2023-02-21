#nullable enable
using System;
using System.Numerics;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Devices.Sensors
{
	/// <summary>
	/// The OrientationSensor API lets you monitor the orientation of a device in three dimensional space.
	/// </summary>
	public interface IOrientationSensor
	{
		/// <summary>
		/// Gets a value indicating whether reading the orientation sensor is supported on this device.
		/// </summary>
		bool IsSupported { get; }

		/// <summary>
		/// Gets a value indicating whether the orientation sensor is actively being monitored.
		/// </summary>
		bool IsMonitoring { get; }

		/// <summary>
		/// Start monitoring for changes to the orientation.
		/// </summary>
		/// <param name="sensorSpeed">The speed to listen for changes.</param>
		void Start(SensorSpeed sensorSpeed);

		/// <summary>
		/// Stop monitoring for changes to the orientation.
		/// </summary>
		void Stop();

		/// <summary>
		/// Occurs when the orientation reading changes.
		/// </summary>
		event EventHandler<OrientationSensorChangedEventArgs> ReadingChanged;
	}

	/// <summary>
	/// The OrientationSensor API lets you monitor the orientation of a device in three dimensional space.
	/// </summary>
	public static class OrientationSensor
	{
		/// <summary>
		/// Occurs when the orientation reading changes.
		/// </summary>
		public static event EventHandler<OrientationSensorChangedEventArgs> ReadingChanged
		{
			add => Current.ReadingChanged += value;
			remove => Current.ReadingChanged -= value;
		}

		/// <summary>
		/// Gets a value indicating whether reading the orientation sensor is supported on this device.
		/// </summary>
		public static bool IsSupported
			=> Current.IsSupported;

		/// <summary>
		/// Gets a value indicating whether the orientation sensor is actively being monitored.
		/// </summary>
		public static bool IsMonitoring { get; private set; }

		/// <summary>
		/// Start monitoring for changes to the orientation.
		/// </summary>
		/// <remarks>
		/// Will throw <see cref="FeatureNotSupportedException"/> if not supported on device.
		/// Will throw <see cref="InvalidOperationException"/> if <see cref="IsMonitoring"/> is <see langword="true"/>.
		/// </remarks>
		/// <param name="sensorSpeed">The speed to listen for changes.</param>
		public static void Start(SensorSpeed sensorSpeed)
			=> Current.Start(sensorSpeed);

		/// <summary>
		/// Stop monitoring for changes to the orientation.
		/// </summary>
		public static void Stop()
			=> Current.Stop();

		static IOrientationSensor Current => Devices.Sensors.OrientationSensor.Default;

		static IOrientationSensor? defaultImplementation;

		/// <summary>
		/// Provides the default implementation for static usage of this API.
		/// </summary>
		public static IOrientationSensor Default =>
			defaultImplementation ??= new OrientationSensorImplementation();

		internal static void SetDefault(IOrientationSensor? implementation) =>
			defaultImplementation = implementation;
	}

	/// <summary>
	/// Contains the current orientation sensor information from the <see cref="IOrientationSensor.ReadingChanged"/> event.
	/// </summary>
	public class OrientationSensorChangedEventArgs : EventArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="OrientationSensorChangedEventArgs"/> class.
		/// </summary>
		/// <param name="reading">The orientation sensor data reading.</param>
		public OrientationSensorChangedEventArgs(OrientationSensorData reading) =>
			Reading = reading;

		/// <summary>
		/// The current values of the orientation sensor.
		/// </summary>
		public OrientationSensorData Reading { get; }
	}

	/// <summary>
	/// Contains the orientation measured by the user's device orientation sensor.
	/// </summary>
	public readonly struct OrientationSensorData : IEquatable<OrientationSensorData>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="OrientationSensorData"/> class.
		/// </summary>
		/// <param name="x">X axis data.</param>
		/// <param name="y">Y axis data.</param>
		/// <param name="z">Z axis data.</param>
		/// <param name="w">W axis data.</param>
		public OrientationSensorData(double x, double y, double z, double w)
			: this((float)x, (float)y, (float)z, (float)w)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="OrientationSensorData"/> class.
		/// </summary>
		/// <param name="x">X axis data.</param>
		/// <param name="y">Y axis data.</param>
		/// <param name="z">Z axis data.</param>
		/// <param name="w">W axis data.</param>
		public OrientationSensorData(float x, float y, float z, float w) =>
			Orientation = new Quaternion(x, y, z, w);

		/// <summary>
		/// Gets the current orientation that represents a <see cref="Quaternion"/>.
		/// </summary>
		public Quaternion Orientation { get; }

		/// <summary>
		/// Compares the underlying <see cref="OrientationSensorData"/> instances.
		/// </summary>
		/// <param name="obj">Object to compare with.</param>
		/// <returns><see langword="true"/> if they are equal, otherwise <see langword="false"/>.</returns>
		public override bool Equals(object? obj) =>
			(obj is OrientationSensorData data) && Equals(data);

		/// <summary>
		/// Compares the underlying <see cref="OrientationSensorData"/> instances.
		/// </summary>
		/// <param name="other"><see cref="OrientationSensorData"/> object to compare with.</param>
		/// <returns><see langword="true"/> if they are equal, otherwise <see langword="false"/>.</returns>
		public bool Equals(OrientationSensorData other) =>
			Orientation.Equals(other.Orientation);

		/// <summary>
		/// Equality operator for equals.
		/// </summary>
		/// <param name="left">Left to compare.</param>
		/// <param name="right">Right to compare.</param>
		/// <returns><see langword="true"/> if objects are equal, otherwise <see langword="false"/>.</returns>
		public static bool operator ==(OrientationSensorData left, OrientationSensorData right) =>
			left.Equals(right);

		/// <summary>
		/// Inequality operator.
		/// </summary>
		/// <param name="left">Left to compare.</param>
		/// <param name="right">Right to compare.</param>
		/// <returns><see langword="true"/> if objects are not equal, otherwise <see langword="false"/>.</returns>
		public static bool operator !=(OrientationSensorData left, OrientationSensorData right) =>
			!left.Equals(right);

		/// <inheritdoc cref="ValueType.GetHashCode"/>
		public override int GetHashCode() =>
			Orientation.GetHashCode();

		/// <summary>
		/// Returns a string representation of the current values of <see cref="Orientation"/>.
		/// </summary>
		/// <returns>A string representation of this instance in the format of <c>Orientation.X: {value}, Orientation.Y: {value}, Orientation.Z: {value}, Orientation.W: {value}</c>.</returns>
		public override string ToString() =>
			$"{nameof(Orientation.X)}: {Orientation.X}, " +
			$"{nameof(Orientation.Y)}: {Orientation.Y}, " +
			$"{nameof(Orientation.Z)}: {Orientation.Z}, " +
			$"{nameof(Orientation.W)}: {Orientation.W}";
	}

	/// <summary>
	/// Concrete implementation of the <see cref="IOrientationSensor"/> APIs.
	/// </summary>
	public partial class OrientationSensorImplementation : IOrientationSensor
	{
		bool UseSyncContext => SensorSpeed == SensorSpeed.Default || SensorSpeed == SensorSpeed.UI;

		SensorSpeed SensorSpeed { get; set; } = SensorSpeed.Default;

		/// <summary>
		/// Occurs when the orientation reading changes.
		/// </summary>
		public event EventHandler<OrientationSensorChangedEventArgs>? ReadingChanged;

		/// <summary>
		/// Gets a value indicating whether reading the orientation sensor is supported on this device.
		/// </summary>
		public bool IsSupported
			=> PlatformIsSupported;

		/// <summary>
		/// Gets a value indicating whether the orientation sensor is actively being monitored.
		/// </summary>
		public bool IsMonitoring { get; private set; }

		/// <summary>
		/// Start monitoring for changes to the orientation.
		/// </summary>
		/// <remarks>
		/// Will throw <see cref="FeatureNotSupportedException"/> if not supported on device.
		/// Will throw <see cref="InvalidOperationException"/> if <see cref="IsMonitoring"/> is <see langword="true"/>.
		/// </remarks>
		/// <param name="sensorSpeed">The speed to listen for changes.</param>
		public void Start(SensorSpeed sensorSpeed)
		{
			if (!PlatformIsSupported)
				throw new FeatureNotSupportedException();

			if (IsMonitoring)
				throw new InvalidOperationException("Orientation sensor has already been started.");

			IsMonitoring = true;
			SensorSpeed = sensorSpeed;

			try
			{
				PlatformStart(sensorSpeed);
			}
			catch
			{
				IsMonitoring = false;
				throw;
			}
		}

		/// <summary>
		/// Stop monitoring for changes to the orientation.
		/// </summary>
		public void Stop()
		{
			if (!PlatformIsSupported)
				throw new FeatureNotSupportedException();

			if (!IsMonitoring)
				return;

			IsMonitoring = false;

			try
			{
				PlatformStop();
			}
			catch
			{
				IsMonitoring = true;
				throw;
			}
		}

		internal void RaiseReadingChanged(OrientationSensorData reading)
		{
			var args = new OrientationSensorChangedEventArgs(reading);

			if (UseSyncContext)
				MainThread.BeginInvokeOnMainThread(() => ReadingChanged?.Invoke(null, args));
			else
				ReadingChanged?.Invoke(null, args);
		}
	}
}
