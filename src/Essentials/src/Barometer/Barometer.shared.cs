#nullable enable
using System;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Devices.Sensors
{
	/// <summary>
	/// Monitor changes to the atmospheric pressure.
	/// </summary>
	public interface IBarometer
	{
		/// <summary>
		/// Gets a value indicating whether reading the barometer is supported on this device.
		/// </summary>
		bool IsSupported { get; }

		/// <summary>
		/// Gets a value indicating whether the barometer is actively being monitored.
		/// </summary>
		bool IsMonitoring { get; }

		/// <summary>
		/// Start monitoring for changes to the barometer.
		/// </summary>
		/// <param name="sensorSpeed">The speed to listen for changes.</param>
		void Start(SensorSpeed sensorSpeed);

		/// <summary>
		/// Occurs when the barometer reading changes.
		/// </summary>
		event EventHandler<BarometerChangedEventArgs>? ReadingChanged;

		/// <summary>
		/// Stop monitoring for changes to the barometer.
		/// </summary>
		void Stop();
	}

	/// <summary>
	/// Monitor changes to the atmospheric pressure.
	/// </summary>
	public static class Barometer
	{
		/// <summary>
		/// Occurs when barometer reading changes.
		/// </summary>
		public static event EventHandler<BarometerChangedEventArgs> ReadingChanged
		{
			add => Default.ReadingChanged += value;
			remove => Default.ReadingChanged -= value;
		}

		/// <summary>
		/// Gets a value indicating whether reading the barometer is supported on this device.
		/// </summary>
		public static bool IsSupported => Default.IsSupported;

		/// <summary>
		/// Gets a value indicating whether the barometer is actively being monitored.
		/// </summary>
		public static bool IsMonitoring
			=> Default.IsMonitoring;

		/// <summary>
		/// Start monitoring for changes to the barometer.
		/// </summary>
		/// <remarks>
		/// Will throw <see cref="FeatureNotSupportedException"/> if not supported on device.
		/// Will throw <see cref="InvalidOperationException"/> if <see cref="IsMonitoring"/> is <see langword="true"/>.
		/// </remarks>
		/// <param name="sensorSpeed">The speed to listen for changes.</param>
		public static void Start(SensorSpeed sensorSpeed)
			=> Default.Start(sensorSpeed);

		/// <summary>
		/// Stop monitoring for changes to the barometer.
		/// </summary>
		public static void Stop()
			=> Default.Stop();

		static IBarometer? defaultImplementation;

		/// <summary>
		/// Provides the default implementation for static usage of this API.
		/// </summary>
		public static IBarometer Default =>
			defaultImplementation ??= new BarometerImplementation();

		internal static void SetDefault(IBarometer? implementation) =>
			defaultImplementation = implementation;
	}

	/// <summary>
	/// Contains the current pressure information from the <see cref="IBarometer.ReadingChanged"/> event.
	/// </summary>
	public class BarometerChangedEventArgs : EventArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="BarometerChangedEventArgs"/> class.
		/// </summary>
		/// <param name="reading">The barometer data reading.</param>
		public BarometerChangedEventArgs(BarometerData reading) =>
			Reading = reading;

		/// <summary>
		/// The current values of the barometer.
		/// </summary>
		public BarometerData Reading { get; }
	}

	/// <summary>
	/// Contains the pressure measured by the user's device barometer.
	/// </summary>
	public readonly struct BarometerData : IEquatable<BarometerData>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="BarometerData"/> class.
		/// </summary>
		/// <param name="pressure">The current pressure reading.</param>
		public BarometerData(double pressure) =>
			PressureInHectopascals = pressure;

		/// <summary>
		/// Gets the current pressure in hectopascals.
		/// </summary>
		public double PressureInHectopascals { get; }

		/// <summary>
		///	Equality operator for equals.
		/// </summary>
		/// <param name="left">Left to compare.</param>
		/// <param name="right">Right to compare.</param>
		/// <returns><see langword="true"/> if objects are equal, otherwise <see langword="false"/>.</returns>
		public static bool operator ==(BarometerData left, BarometerData right) =>
			left.Equals(right);

		/// <summary>
		/// Inequality operator.
		/// </summary>
		/// <param name="left">Left to compare.</param>
		/// <param name="right">Right to compare.</param>
		/// <returns><see langword="true"/> if objects are not equal, otherwise <see langword="false"/>.</returns>
		public static bool operator !=(BarometerData left, BarometerData right) =>
			!left.Equals(right);

		/// <summary>
		/// Compares the underlying <see cref="BarometerData"/> instances.
		/// </summary>
		/// <param name="obj">Object to compare with.</param>
		/// <returns><see langword="true"/> if they are equal, otherwise <see langword="false"/>.</returns>
		public override bool Equals(object? obj) =>
			(obj is BarometerData data) && Equals(data);

		/// <summary>
		/// Compares the underlying <see cref="BarometerData.PressureInHectopascals"/> instances.
		/// </summary>
		/// <param name="other"><see cref="BarometerData"/> object to compare with.</param>
		/// <returns><see langword="true"/> if they are equal, otherwise <see langword="false"/>.</returns>
		public bool Equals(BarometerData other) =>
			PressureInHectopascals.Equals(other.PressureInHectopascals);

		/// <inheritdoc cref="object.GetHashCode"/>
		public override int GetHashCode() =>
			PressureInHectopascals.GetHashCode();

		/// <summary>
		/// Returns a string representation of the current values of <see cref="PressureInHectopascals"/>.
		/// </summary>
		/// <returns>A string representation of this instance in the format of <c>PressureInHectopascals: {value}</c>.</returns>
		public override string ToString() => $"{nameof(PressureInHectopascals)}: {PressureInHectopascals}";
	}

	partial class BarometerImplementation : IBarometer
	{
		bool UseSyncContext => SensorSpeed == SensorSpeed.Default || SensorSpeed == SensorSpeed.UI;

#pragma warning disable CS0067
		public event EventHandler<BarometerChangedEventArgs>? ReadingChanged;
#pragma warning restore CS0067

		public bool IsMonitoring { get; private set; }

		SensorSpeed SensorSpeed { get; set; } = SensorSpeed.Default;

		void RaiseReadingChanged(BarometerData reading)
		{
			var args = new BarometerChangedEventArgs(reading);

			if (UseSyncContext)
				MainThread.BeginInvokeOnMainThread(() => ReadingChanged?.Invoke(this, args));
			else
				ReadingChanged?.Invoke(this, args);
		}

		/// <inheritdoc/>
		/// <exception cref="FeatureNotSupportedException">Thrown if <see cref="IsSupported"/> returns <see langword="false"/>.</exception>
		/// <exception cref="InvalidOperationException">Thrown if <see cref="IsMonitoring"/> returns <see langword="true"/>.</exception>
		public void Start(SensorSpeed sensorSpeed)
		{
			if (!IsSupported)
				throw new FeatureNotSupportedException();

			if (IsMonitoring)
				throw new InvalidOperationException("Barometer has already been started.");

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

		/// <inheritdoc/>
		/// <exception cref="FeatureNotSupportedException">Thrown if <see cref="IsSupported"/> returns <see langword="false"/>.</exception>
		public void Stop()
		{
			if (!IsSupported)
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
	}
}
