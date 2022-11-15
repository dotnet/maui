#nullable enable
using System;
using System.Numerics;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Devices.Sensors
{
	/// <summary>
	/// Accelerometer data of the acceleration of the device in three dimensional space.
	/// </summary>
	public interface IAccelerometer
	{
		/// <summary>
		/// Event triggered when reading of sensor changes.
		/// </summary>
		event EventHandler<AccelerometerChangedEventArgs>? ReadingChanged;

		/// <summary>
		/// Event triggered when a shake has been detected on the device.
		/// </summary>
		event EventHandler? ShakeDetected;

		/// <summary>
		/// Gets if reading the accelerometer is supported on this device.
		/// </summary>
		bool IsSupported { get; }

		/// <summary>
		/// Gets if accelerometer is being monitored.
		/// </summary>
		bool IsMonitoring { get; }

		/// <summary>
		/// Start monitoring for changes to accelerometer.
		/// </summary>
		/// <remarks>Will throw <see cref="FeatureNotSupportedException"/> if not supported on device. Will throw <see cref="ArgumentNullException"/> if handler is null.</remarks>
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
		/// <inheritdoc/>
		public static event EventHandler<AccelerometerChangedEventArgs> ReadingChanged
		{
			add => Default.ReadingChanged += value;
			remove => Default.ReadingChanged -= value;
		}

		/// <inheritdoc/>
		public static event EventHandler ShakeDetected
		{
			add => Default.ShakeDetected += value;
			remove => Default.ShakeDetected -= value;
		}

		/// <inheritdoc/>
		public static bool IsSupported
			=> Default.IsSupported;

		/// <inheritdoc/>
		public static bool IsMonitoring => Default.IsMonitoring;

		/// <inheritdoc/>
		public static void Start(SensorSpeed sensorSpeed) => Default.Start(sensorSpeed);

		/// <inheritdoc/>
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

		public override bool Equals(object? obj) =>
			(obj is AccelerometerData data) && Equals(data);

		/// <summary>
		/// Compares the underlying <see cref="Vector3"/> instances.
		/// </summary>
		/// <param name="other"><see cref="AccelerometerData"/> object to compare with.</param>
		/// <returns><see langword="true"/> if they are equal, otherwise <see langword="false"/>.</returns>
		public bool Equals(AccelerometerData other) =>
			Acceleration.Equals(other.Acceleration);

		public static bool operator ==(AccelerometerData left, AccelerometerData right) =>
			left.Equals(right);

		public static bool operator !=(AccelerometerData left, AccelerometerData right) =>
			!left.Equals(right);

		/// <include file="../../docs/Microsoft.Maui.Essentials/AccelerometerData.xml" path="//Member[@MemberName='GetHashCode']/Docs/*" />
		public override int GetHashCode() =>
			Acceleration.GetHashCode();

		/// <include file="../../docs/Microsoft.Maui.Essentials/AccelerometerData.xml" path="//Member[@MemberName='ToString']/Docs/*" />
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