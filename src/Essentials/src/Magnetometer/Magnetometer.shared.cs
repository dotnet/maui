using System;
using System.Numerics;

namespace Microsoft.Maui.Essentials
{
	public static partial class Magnetometer
	{
		static bool useSyncContext;

		public static event EventHandler<MagnetometerChangedEventArgs> ReadingChanged;

		public static bool IsMonitoring { get; private set; }

		public static void Start(SensorSpeed sensorSpeed)
		{
			if (!IsSupported)
				throw new FeatureNotSupportedException();

			if (IsMonitoring)
				throw new InvalidOperationException("Magnetometer has already been started.");

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

		public static void Stop()
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

		internal static void OnChanged(MagnetometerData reading) =>
			OnChanged(new MagnetometerChangedEventArgs(reading));

		internal static void OnChanged(MagnetometerChangedEventArgs e)
		{
			if (useSyncContext)
				MainThread.BeginInvokeOnMainThread(() => ReadingChanged?.Invoke(null, e));
			else
				ReadingChanged?.Invoke(null, e);
		}
	}

	public class MagnetometerChangedEventArgs : EventArgs
	{
		public MagnetometerChangedEventArgs(MagnetometerData reading) =>
			Reading = reading;

		public MagnetometerData Reading { get; }
	}

	public readonly struct MagnetometerData : IEquatable<MagnetometerData>
	{
		public MagnetometerData(double x, double y, double z)
			: this((float)x, (float)y, (float)z)
		{
		}

		public MagnetometerData(float x, float y, float z) =>
			MagneticField = new Vector3(x, y, z);

		public Vector3 MagneticField { get; }

		public override bool Equals(object obj) =>
			(obj is MagnetometerData data) && Equals(data);

		public bool Equals(MagnetometerData other) =>
			MagneticField.Equals(other.MagneticField);

		public static bool operator ==(MagnetometerData left, MagnetometerData right) =>
			left.Equals(right);

		public static bool operator !=(MagnetometerData left, MagnetometerData right) =>
		   !left.Equals(right);

		public override int GetHashCode() =>
			MagneticField.GetHashCode();

		public override string ToString() =>
			$"{nameof(MagneticField.X)}: {MagneticField.X}, " +
			$"{nameof(MagneticField.Y)}: {MagneticField.Y}, " +
			$"{nameof(MagneticField.Z)}: {MagneticField.Z}";
	}
}
