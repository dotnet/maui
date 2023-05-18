#nullable enable
using System;
using System.Numerics;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Devices.Sensors
{
	/// <summary>
	/// Detect device's orentation relative to Earth's magnetic field in microteslas (µ).
	/// </summary>
	public interface IMagnetometer
	{
		/// <summary>
		/// Gets a value indicating whether reading the magnetometer is supported on this device.
		/// </summary>
		bool IsSupported { get; }

		/// <summary>
		/// Gets a value indicating whether the magnetometer is actively being monitored.
		/// </summary>
		bool IsMonitoring { get; }

		/// <summary>
		/// Start monitoring for changes to the magnetometer.
		/// </summary>
		/// <param name="sensorSpeed">The speed to listen for changes.</param>
		void Start(SensorSpeed sensorSpeed);

		/// <summary>
		/// Stop monitoring for changes to the magnetometer.
		/// </summary>
		void Stop();

		/// <summary>
		/// Occurs when the magnetometer reading changes.
		/// </summary>
		event EventHandler<MagnetometerChangedEventArgs> ReadingChanged;
	}

	/// <summary>
	/// Detect device's orentation relative to Earth's magnetic field in microteslas (µ).
	/// </summary>
	public static partial class Magnetometer
	{
		/// <summary>
		/// Occurs when the magnetometer reading changes.
		/// </summary>
		public static event EventHandler<MagnetometerChangedEventArgs> ReadingChanged
		{
			add => Current.ReadingChanged += value;
			remove => Current.ReadingChanged -= value;
		}

		/// <summary>
		/// Gets a value indicating whether the magnetometer is actively being monitored.
		/// </summary>
		public static bool IsMonitoring
			=> Current.IsMonitoring;

		/// <summary>
		/// Gets a value indicating whether reading the magnetometer is supported on this device.
		/// </summary>
		public static bool IsSupported
			=> Current.IsSupported;

		/// <summary>
		/// Start monitoring for changes to the magnetometer.
		/// </summary>
		/// <param name="sensorSpeed">The speed to listen for changes.</param>
		/// <remarks>
		/// Will throw <see cref="FeatureNotSupportedException"/> if not supported on device.
		/// Will throw <see cref="InvalidOperationException"/> if <see cref="IsMonitoring"/> is <see langword="true"/>.
		/// </remarks>
		public static void Start(SensorSpeed sensorSpeed)
			=> Current.Start(sensorSpeed);

		/// <summary>
		/// Stop monitoring for changes to the magnetometer.
		/// </summary>
		public static void Stop()
			=> Current.Stop();

		static IMagnetometer Current => Devices.Sensors.Magnetometer.Default;

		static IMagnetometer? defaultImplementation;

		/// <summary>
		/// Provides the default implementation for static usage of this API.
		/// </summary>
		public static IMagnetometer Default =>
			defaultImplementation ??= new MagnetometerImplementation();

		internal static void SetDefault(IMagnetometer? implementation) =>
			defaultImplementation = implementation;
	}

	/// <summary>
	/// Contains the current pressure information from the <see cref="IBarometer.ReadingChanged"/> event.
	/// </summary>
	public class MagnetometerChangedEventArgs : EventArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MagnetometerChangedEventArgs"/> class.
		/// </summary>
		/// <param name="reading">The magnetometer data reading.</param>
		public MagnetometerChangedEventArgs(MagnetometerData reading) =>
			Reading = reading;

		/// <summary>
		/// The current values of the magnetometer.
		/// </summary>
		public MagnetometerData Reading { get; }
	}

	/// <summary>
	/// Contains the pressure measured by the user's device magnetometer.
	/// </summary>
	public readonly struct MagnetometerData : IEquatable<MagnetometerData>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MagnetometerData"/> class.
		/// </summary>
		/// <param name="x">X axis data.</param>
		/// <param name="y">Y axis data.</param>
		/// <param name="z">Z asix data.</param>
		public MagnetometerData(double x, double y, double z)
			: this((float)x, (float)y, (float)z)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MagnetometerData"/> class.
		/// </summary>
		/// <param name="x">X axis data.</param>
		/// <param name="y">Y axis data.</param>
		/// <param name="z">Z asix data.</param>
		public MagnetometerData(float x, float y, float z) =>
			MagneticField = new Vector3(x, y, z);

		/// <summary>
		/// Gets the magnetic field vector in microteslas (µ).
		/// </summary>
		public Vector3 MagneticField { get; }

		/// <summary>
		/// Compares the underlying <see cref="MagnetometerData"/> instances.
		/// </summary>
		/// <param name="obj">Object to compare with.</param>
		/// <returns><see langword="true"/> if they are equal, otherwise <see langword="false"/>.</returns>
		public override bool Equals(object? obj) =>
			(obj is MagnetometerData data) && Equals(data);

		/// <summary>
		/// Compares the underlying <see cref="MagneticField"/> instances.
		/// </summary>
		/// <param name="other"><see cref="MagnetometerData"/> object to compare with.</param>
		/// <returns><see langword="true"/> if they are equal, otherwise <see langword="false"/>.</returns>
		public bool Equals(MagnetometerData other) =>
			MagneticField.Equals(other.MagneticField);

		/// <summary>
		/// Equality operator for equals.
		/// </summary>
		/// <param name="left">Left to compare.</param>
		/// <param name="right">Right to compare.</param>
		/// <returns><see langword="true"/> if objects are equal, otherwise <see langword="false"/>.</returns>
		public static bool operator ==(MagnetometerData left, MagnetometerData right) =>
			left.Equals(right);

		/// <summary>
		/// Inequality operator.
		/// </summary>
		/// <param name="left">Left to compare.</param>
		/// <param name="right">Right to compare.</param>
		/// <returns><see langword="true"/> if objects are not equal, otherwise <see langword="false"/>.</returns>
		public static bool operator !=(MagnetometerData left, MagnetometerData right) =>
		   !left.Equals(right);

		/// <inheritdoc cref="object.GetHashCode"/>
		public override int GetHashCode() =>
			MagneticField.GetHashCode();

		/// <summary>
		/// Returns a string representation of the current values of <see cref="MagneticField"/>.
		/// </summary>
		/// <returns>A string representation of this instance in the format of <c>MagneticField.X: {value}, MagneticField.Y: {value}, MagneticField.Z: {value}</c>.</returns>
		public override string ToString() =>
			$"{nameof(MagneticField.X)}: {MagneticField.X}, " +
			$"{nameof(MagneticField.Y)}: {MagneticField.Y}, " +
			$"{nameof(MagneticField.Z)}: {MagneticField.Z}";
	}

	partial class MagnetometerImplementation : IMagnetometer
	{
		bool UseSyncContext => SensorSpeed == SensorSpeed.Default || SensorSpeed == SensorSpeed.UI;

		public event EventHandler<MagnetometerChangedEventArgs>? ReadingChanged;

		public bool IsMonitoring { get; private set; }

		public bool IsSupported => PlatformIsSupported;

		SensorSpeed SensorSpeed { get; set; } = SensorSpeed.Default;

		public void Start(SensorSpeed sensorSpeed)
		{
			if (!PlatformIsSupported)
				throw new FeatureNotSupportedException();

			if (IsMonitoring)
				throw new InvalidOperationException("Magnetometer has already been started.");

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

		void RaiseReadingChanged(MagnetometerData data)
		{
			var args = new MagnetometerChangedEventArgs(data);

			if (UseSyncContext)
				MainThread.BeginInvokeOnMainThread(() => ReadingChanged?.Invoke(this, args));
			else
				ReadingChanged?.Invoke(this, args);
		}
	}
}
