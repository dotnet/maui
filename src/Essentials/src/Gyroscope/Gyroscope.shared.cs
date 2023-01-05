#nullable enable
using System;
using System.Numerics;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Devices.Sensors
{
	/// <summary>
	/// The Gyroscope API lets you monitor the device's gyroscope sensor which is the rotation around the device's three primary axes.
	/// </summary>
	public interface IGyroscope
	{
		/// <summary>
		/// Gets a value indicating whether reading the gyroscope is supported on this device.
		/// </summary>
		bool IsSupported { get; }

		/// <summary>
		/// Gets a value indicating whether the gyroscope is actively being monitored.
		/// </summary>
		bool IsMonitoring { get; }

		/// <summary>
		/// Start monitoring for changes to the gyroscope.
		/// </summary>
		/// <param name="sensorSpeed">The speed to listen for changes.</param>
		void Start(SensorSpeed sensorSpeed);

		/// <summary>
		/// Stop monitoring for changes to the gyroscope.
		/// </summary>
		void Stop();

		/// <summary>
		/// Occurs when the gyroscope reading changes.
		/// </summary>
		event EventHandler<GyroscopeChangedEventArgs> ReadingChanged;
	}

	/// <summary>
	/// The Gyroscope API lets you monitor the device's gyroscope sensor which is the rotation around the device's three primary axes.
	/// </summary>
	public static partial class Gyroscope
	{
		/// <summary>
		/// Occurs when the gyroscope reading changes.
		/// </summary>
		public static event EventHandler<GyroscopeChangedEventArgs> ReadingChanged
		{
			add => Current.ReadingChanged += value;
			remove => Current.ReadingChanged -= value;
		}

		/// <summary>
		/// Gets a value indicating whether the gyroscope is actively being monitored.
		/// </summary>
		public static bool IsMonitoring
			=> Current.IsMonitoring;

		/// <summary>
		/// Gets a value indicating whether reading the gyroscope is supported on this device.
		/// </summary>
		public static bool IsSupported
			=> Current.IsSupported;

		/// <summary>
		/// Start monitoring for changes to the gyroscope.
		/// </summary>
		/// <param name="sensorSpeed">The speed to listen for changes.</param>
		public static void Start(SensorSpeed sensorSpeed)
			=> Current.Start(sensorSpeed);

		/// <summary>
		/// Stop monitoring for changes to the gyroscope.
		/// </summary>
		public static void Stop()
			=> Current.Stop();

		static IGyroscope Current => Devices.Sensors.Gyroscope.Default;

		static IGyroscope? defaultImplementation;

		/// <summary>
		/// Provides the default implementation for static usage of this API.
		/// </summary>
		public static IGyroscope Default =>
			defaultImplementation ??= new GyroscopeImplementation();

		internal static void SetDefault(IGyroscope? implementation) =>
			defaultImplementation = implementation;
	}

	/// <summary>
	/// Contains the current axis reading information from the <see cref="IGyroscope.ReadingChanged"/> event.
	/// </summary>
	public class GyroscopeChangedEventArgs : EventArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="GyroscopeChangedEventArgs"/> class.
		/// </summary>
		/// <param name="reading">The gyroscope data reading.</param>
		public GyroscopeChangedEventArgs(GyroscopeData reading) =>
			Reading = reading;

		/// <summary>
		/// The current values of the gyroscope.
		/// </summary>
		public GyroscopeData Reading { get; }
	}

	/// <summary>
	/// Contains the axis readings measured by the device's gyroscope.
	/// </summary>
	public readonly struct GyroscopeData : IEquatable<GyroscopeData>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="GyroscopeData"/> class.
		/// </summary>
		/// <param name="x">X axis data.</param>
		/// <param name="y">Y axis data.</param>
		/// <param name="z">Z axis data.</param>
		public GyroscopeData(double x, double y, double z)
			: this((float)x, (float)y, (float)z)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GyroscopeData"/> class.
		/// </summary>
		/// <param name="x">X axis data.</param>
		/// <param name="y">Y axis data.</param>
		/// <param name="z">Z axis data.</param>
		public GyroscopeData(float x, float y, float z) =>
			AngularVelocity = new Vector3(x, y, z);

		/// <summary>
		/// Gets the angular velocity vector in radians per second.
		/// </summary>
		public Vector3 AngularVelocity { get; }

		/// <summary>
		/// Compares the underlying <see cref="GyroscopeData"/> instances.
		/// </summary>
		/// <param name="obj">Object to compare with.</param>
		/// <returns><see langword="true"/> if they are equal, otherwise <see langword="false"/>.</returns>
		public override bool Equals(object? obj) =>
			(obj is GyroscopeData data) && Equals(data);

		/// <summary>
		/// Compares the underlying <see cref="GyroscopeData.AngularVelocity"/> instances.
		/// </summary>
		/// <param name="other"><see cref="GyroscopeData"/> object to compare with.</param>
		/// <returns><see langword="true"/> if they are equal, otherwise <see langword="false"/>.</returns>
		public bool Equals(GyroscopeData other) =>
			AngularVelocity.Equals(other.AngularVelocity);

		/// <summary>
		/// Equality operator for equals.
		/// </summary>
		/// <param name="left">Left to compare.</param>
		/// <param name="right">Right to compare.</param>
		/// <returns><see langword="true"/> if objects are equal, otherwise <see langword="false"/>.</returns>
		public static bool operator ==(GyroscopeData left, GyroscopeData right) =>
		  left.Equals(right);

		/// <summary>
		/// Inequality operator.
		/// </summary>
		/// <param name="left">Left to compare.</param>
		/// <param name="right">Right to compare.</param>
		/// <returns><see langword="true"/> if objects are not equal, otherwise <see langword="false"/>.</returns>
		public static bool operator !=(GyroscopeData left, GyroscopeData right) =>
		   !left.Equals(right);

		/// <inheritdoc cref="ValueType.GetHashCode"/>
		public override int GetHashCode() =>
			AngularVelocity.GetHashCode();

		/// <summary>
		/// Returns a string representation of the current values of <see cref="AngularVelocity"/>.
		/// </summary>
		/// <returns>A string representation of this instance in the format of <c>AngularVelocity.X: {value}, AngularVelocity.Y: {value}, AngularVelocity.Z: {value}</c>.</returns>
		public override string ToString() =>
			$"{nameof(AngularVelocity.X)}: {AngularVelocity.X}, " +
			$"{nameof(AngularVelocity.Y)}: {AngularVelocity.Y}, " +
			$"{nameof(AngularVelocity.Z)}: {AngularVelocity.Z}";
	}

	partial class GyroscopeImplementation : IGyroscope
	{
		bool UseSyncContext => SensorSpeed == SensorSpeed.Default || SensorSpeed == SensorSpeed.UI;

		SensorSpeed SensorSpeed { get; set; } = SensorSpeed.Default;

		public event EventHandler<GyroscopeChangedEventArgs>? ReadingChanged;

		public bool IsMonitoring { get; private set; }

		public bool IsSupported => PlatformIsSupported;

		public void Start(SensorSpeed sensorSpeed)
		{
			if (!PlatformIsSupported)
				throw new FeatureNotSupportedException();

			if (IsMonitoring)
				throw new InvalidOperationException("Gyroscope has already been started.");

			IsMonitoring = true;

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

		void RaiseReadingChanged(GyroscopeData data)
		{
			var args = new GyroscopeChangedEventArgs(data);

			if (UseSyncContext)
				MainThread.BeginInvokeOnMainThread(() => ReadingChanged?.Invoke(null, args));
			else
				ReadingChanged?.Invoke(null, args);
		}
	}
}
