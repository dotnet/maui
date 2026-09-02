#nullable enable
using System;
using System.Numerics;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Devices.Sensors
{
	/// <summary>
	/// Accelerometer data of the acceleration of the device in three-dimensional space.
	/// </summary>
	public interface IAccelerometer
	{
		/// <summary>
		/// Occurs when the sensor reading changes.
		/// </summary>
		event EventHandler<AccelerometerChangedEventArgs>? ReadingChanged;

		/// <summary>
		/// Occurs when the accelerometer detects that the device has been shaken.
		/// </summary>
		event EventHandler? ShakeDetected;

		/// <summary>
		/// Gets a value indicating whether reading the accelerometer is supported on this device.
		/// </summary>
		bool IsSupported { get; }

		/// <summary>
		/// Gets a value indicating whether the accelerometer is being monitored.
		/// </summary>
		bool IsMonitoring { get; }

		/// <summary>
		/// Start monitoring for changes to accelerometer.
		/// </summary>
		/// <remarks>
		/// Will throw <see cref="FeatureNotSupportedException"/> if <see cref="IsSupported"/> is <see langword="false"/>.
		/// Will throw <see cref="InvalidOperationException"/> if <see cref="IsMonitoring"/> is <see langword="true"/>.</remarks>
		/// <param name="sensorSpeed">Speed to monitor the sensor.</param>
		void Start(SensorSpeed sensorSpeed);

		/// <summary>
		/// Stop monitoring for changes to accelerometer.
		/// </summary>
		void Stop();
	}

	/// <summary>
	/// Accelerometer data of the acceleration of the device in three dimensional space.
	/// </summary>
	public static class Accelerometer
	{
		/// <summary>
		/// Occurs when the accelerometer reading changes.
		/// </summary>
		public static event EventHandler<AccelerometerChangedEventArgs> ReadingChanged
		{
			add => Default.ReadingChanged += value;
			remove => Default.ReadingChanged -= value;
		}

		/// <summary>
		/// Occurs when the accelerometer detects that the device has been shaken.
		/// </summary>
		public static event EventHandler ShakeDetected
		{
			add => Default.ShakeDetected += value;
			remove => Default.ShakeDetected -= value;
		}

		/// <summary>
		/// Gets a value indicating whether reading the accelerometer is supported on this device.
		/// </summary>
		public static bool IsSupported
			=> Default.IsSupported;

		/// <summary>
		/// Gets a value indicating whether the accelerometer is being monitored.
		/// </summary>
		public static bool IsMonitoring => Default.IsMonitoring;

		/// <summary>
		/// Start monitoring for changes to accelerometer.
		/// </summary>
		/// <remarks>Will throw <see cref="FeatureNotSupportedException"/> if not supported on device. Will throw <see cref="ArgumentNullException"/> if handler is null.</remarks>
		/// <param name="sensorSpeed">Speed to monitor the sensor.</param>
		public static void Start(SensorSpeed sensorSpeed) => Default.Start(sensorSpeed);

		/// <summary>
		/// Stop monitoring for changes to accelerometer.
		/// </summary>
		public static void Stop() => Default.Stop();

		static IAccelerometer? defaultImplementation;

		/// <summary>
		/// Provides the default implementation for static usage of this API.
		/// </summary>
		public static IAccelerometer Default =>
			defaultImplementation ??= new AccelerometerImplementation();

		internal static void SetDefault(IAccelerometer? implementation) =>
			defaultImplementation = implementation;
	}

	/// <summary>
	/// Event arguments containing the current reading of <see cref="IAccelerometer"/>.
	/// </summary>
	public class AccelerometerChangedEventArgs : EventArgs
	{
		/// <summary>
		/// Public constructor that takes in a reading for event arguments.
		/// </summary>
		/// <param name="reading">The accelerometer data reading.</param>
		public AccelerometerChangedEventArgs(AccelerometerData reading) => Reading = reading;

		/// <summary>
		/// The current values of accelerometer.
		/// </summary>
		public AccelerometerData Reading { get; }
	}

	/// <summary>
	/// Data representing the devices' three accelerometers.
	/// </summary>
	public readonly struct AccelerometerData : IEquatable<AccelerometerData>
	{
		/// <summary>
		/// Public constructor for accelerometer data.
		/// </summary>
		/// <param name="x">X data</param>
		/// <param name="y">Y data</param>
		/// <param name="z">Z data</param>
		public AccelerometerData(double x, double y, double z)
			: this((float)x, (float)y, (float)z)
		{
		}

		/// <summary>
		/// Public constructor for accelerometer data.
		/// </summary>
		/// <param name="x">X data</param>
		/// <param name="y">Y data</param>
		/// <param name="z">Z data</param>
		public AccelerometerData(float x, float y, float z) =>
			Acceleration = new Vector3(x, y, z);

		/// <summary>
		/// Gets the acceleration vector in G's (gravitational force).
		/// </summary>
		public Vector3 Acceleration { get; }

		/// <inheritdoc cref="IEquatable{T}.Equals(T)"/>
		public override bool Equals(object? obj) =>
			(obj is AccelerometerData data) && Equals(data);

		/// <summary>
		/// Compares the underlying <see cref="Vector3"/> instances.
		/// </summary>
		/// <param name="other"><see cref="AccelerometerData"/> object to compare with.</param>
		/// <returns><see langword="true"/> if they are equal, otherwise <see langword="false"/>.</returns>
		public bool Equals(AccelerometerData other) =>
			Acceleration.Equals(other.Acceleration);

		/// <summary>
		///	Equality operator for equals.
		/// </summary>
		/// <param name="left">Left to compare.</param>
		/// <param name="right">Right to compare.</param>
		/// <returns><see langword="true"/> if objects are equal, otherwise <see langword="false"/>.</returns>
		public static bool operator ==(AccelerometerData left, AccelerometerData right) =>
			left.Equals(right);

		/// <summary>
		/// Inequality operator.
		/// </summary>
		/// <param name="left">Left to compare.</param>
		/// <param name="right">Right to compare.</param>
		/// <returns><see langword="true"/> if objects are not equal, otherwise <see langword="false"/>.</returns>
		public static bool operator !=(AccelerometerData left, AccelerometerData right) =>
			!left.Equals(right);

		/// <inheritdoc cref="object.GetHashCode"/>
		public override int GetHashCode() =>
			Acceleration.GetHashCode();

		/// <summary>
		/// Returns a string representation of the current values of <see cref="Acceleration"/>.
		/// </summary>
		/// <returns>A string representation of this instance in the format of <c>X: x, Y: y, Z: z</c>.</returns>
		public override string ToString() =>
			$"{nameof(Acceleration.X)}: {Acceleration.X}, " +
			$"{nameof(Acceleration.Y)}: {Acceleration.Y}, " +
			$"{nameof(Acceleration.Z)}: {Acceleration.Z}";
	}

	partial class AccelerometerImplementation : IAccelerometer
	{
		const double accelerationThreshold = 169;

		const double gravity = 9.81;

		static readonly AccelerometerQueue queue = new AccelerometerQueue();

		static bool useSyncContext;

		/// <inheritdoc/>
		public event EventHandler<AccelerometerChangedEventArgs>? ReadingChanged;

		/// <inheritdoc/>
		public event EventHandler? ShakeDetected;

		/// <inheritdoc/>
		public bool IsMonitoring { get; private set; }

		/// <inheritdoc/>
		/// <exception cref="FeatureNotSupportedException">Thrown if <see cref="IsSupported"/> returns <see langword="false"/>.</exception>
		/// <exception cref="InvalidOperationException">Thrown if <see cref="IsMonitoring"/> returns <see langword="true"/>.</exception>
		public void Start(SensorSpeed sensorSpeed)
		{
			if (!IsSupported)
				throw new FeatureNotSupportedException();

			if (IsMonitoring)
				throw new InvalidOperationException("Accelerometer has already been started.");

			IsMonitoring = true;
			useSyncContext = sensorSpeed == SensorSpeed.Default || sensorSpeed == SensorSpeed.UI;

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

		internal void OnChanged(AccelerometerData reading) =>
			OnChanged(new AccelerometerChangedEventArgs(reading));

		internal void OnChanged(AccelerometerChangedEventArgs e)
		{
			if (useSyncContext)
				MainThread.BeginInvokeOnMainThread(() => ReadingChanged?.Invoke(null, e));
			else
				ReadingChanged?.Invoke(null, e);

			if (ShakeDetected != null)
				ProcessShakeEvent(e.Reading.Acceleration);
		}

		void ProcessShakeEvent(Vector3 acceleration)
		{
			var now = Nanoseconds(DateTime.UtcNow);

			var x = acceleration.X * gravity;
			var y = acceleration.Y * gravity;
			var z = acceleration.Z * gravity;

			var g = x * x + y * y + z * z;
			queue.Add(now, g > accelerationThreshold);

			if (queue.IsShaking)
			{
				queue.Clear();
				var args = new EventArgs();

				if (useSyncContext)
					MainThread.BeginInvokeOnMainThread(() => ShakeDetected?.Invoke(null, args));
				else
					ShakeDetected?.Invoke(null, args);
			}

			static long Nanoseconds(DateTime time) =>
				(time.Ticks / TimeSpan.TicksPerMillisecond) * 1_000_000;
		}
	}
}