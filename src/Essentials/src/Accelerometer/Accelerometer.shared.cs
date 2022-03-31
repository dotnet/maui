#nullable enable
using System;
using System.Numerics;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Devices.Sensors
{
	public interface IAccelerometer
	{
		event EventHandler<AccelerometerChangedEventArgs>? ReadingChanged;

		event EventHandler? ShakeDetected;

		bool IsSupported { get; }

		bool IsMonitoring { get; }

		void Start(SensorSpeed sensorSpeed);

		void Stop();
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/Accelerometer.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Accelerometer']/Docs" />
	public static class Accelerometer
	{
		public static event EventHandler<AccelerometerChangedEventArgs> ReadingChanged
		{
			add => Default.ReadingChanged += value;
			remove => Default.ReadingChanged -= value;
		}

		public static event EventHandler ShakeDetected
		{
			add => Default.ShakeDetected += value;
			remove => Default.ShakeDetected -= value;
		}

		public static bool IsSupported
			=> Default.IsSupported;

		/// <include file="../../docs/Microsoft.Maui.Essentials/Accelerometer.xml" path="//Member[@MemberName='IsMonitoring']/Docs" />
		public static bool IsMonitoring => Default.IsMonitoring;

		/// <include file="../../docs/Microsoft.Maui.Essentials/Accelerometer.xml" path="//Member[@MemberName='Start']/Docs" />
		public static void Start(SensorSpeed sensorSpeed) => Default.Start(sensorSpeed);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Accelerometer.xml" path="//Member[@MemberName='Stop']/Docs" />
		public static void Stop() => Default.Stop();

		static IAccelerometer? defaultImplementation;

		public static IAccelerometer Default =>
			defaultImplementation ??= new AccelerometerImplementation();

		internal static void SetDefault(IAccelerometer? implementation) =>
			defaultImplementation = implementation;
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/AccelerometerChangedEventArgs.xml" path="Type[@FullName='Microsoft.Maui.Essentials.AccelerometerChangedEventArgs']/Docs" />
	public class AccelerometerChangedEventArgs : EventArgs
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/AccelerometerChangedEventArgs.xml" path="//Member[@MemberName='.ctor']/Docs" />
		public AccelerometerChangedEventArgs(AccelerometerData reading) => Reading = reading;

		/// <include file="../../docs/Microsoft.Maui.Essentials/AccelerometerChangedEventArgs.xml" path="//Member[@MemberName='Reading']/Docs" />
		public AccelerometerData Reading { get; }
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/AccelerometerData.xml" path="Type[@FullName='Microsoft.Maui.Essentials.AccelerometerData']/Docs" />
	public readonly struct AccelerometerData : IEquatable<AccelerometerData>
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/AccelerometerData.xml" path="//Member[@MemberName='.ctor'][1]/Docs" />
		public AccelerometerData(double x, double y, double z)
			: this((float)x, (float)y, (float)z)
		{
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/AccelerometerData.xml" path="//Member[@MemberName='.ctor'][2]/Docs" />
		public AccelerometerData(float x, float y, float z) =>
			Acceleration = new Vector3(x, y, z);

		/// <include file="../../docs/Microsoft.Maui.Essentials/AccelerometerData.xml" path="//Member[@MemberName='Acceleration']/Docs" />
		public Vector3 Acceleration { get; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/AccelerometerData.xml" path="//Member[@MemberName='Equals'][1]/Docs" />
		public override bool Equals(object? obj) =>
			(obj is AccelerometerData data) && Equals(data);

		/// <include file="../../docs/Microsoft.Maui.Essentials/AccelerometerData.xml" path="//Member[@MemberName='Equals'][2]/Docs" />
		public bool Equals(AccelerometerData other) =>
			Acceleration.Equals(other.Acceleration);

		public static bool operator ==(AccelerometerData left, AccelerometerData right) =>
			left.Equals(right);

		public static bool operator !=(AccelerometerData left, AccelerometerData right) =>
			!left.Equals(right);

		/// <include file="../../docs/Microsoft.Maui.Essentials/AccelerometerData.xml" path="//Member[@MemberName='GetHashCode']/Docs" />
		public override int GetHashCode() =>
			Acceleration.GetHashCode();

		/// <include file="../../docs/Microsoft.Maui.Essentials/AccelerometerData.xml" path="//Member[@MemberName='ToString']/Docs" />
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

		public event EventHandler<AccelerometerChangedEventArgs>? ReadingChanged;

		public event EventHandler? ShakeDetected;

		public bool IsMonitoring { get; private set; }

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