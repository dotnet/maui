using System;
using System.Numerics;

namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/OrientationSensor.xml" path="Type[@FullName='Microsoft.Maui.Essentials.OrientationSensor']/Docs" />
	public static partial class OrientationSensor
	{
		static bool useSyncContext;

		public static event EventHandler<OrientationSensorChangedEventArgs> ReadingChanged;

		/// <include file="../../docs/Microsoft.Maui.Essentials/OrientationSensor.xml" path="//Member[@MemberName='IsMonitoring']/Docs" />
		public static bool IsMonitoring { get; private set; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/OrientationSensor.xml" path="//Member[@MemberName='Start']/Docs" />
		public static void Start(SensorSpeed sensorSpeed)
		{
			if (!IsSupported)
				throw new FeatureNotSupportedException();

			if (IsMonitoring)
				throw new InvalidOperationException("Orientation sensor has already been started.");

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

		/// <include file="../../docs/Microsoft.Maui.Essentials/OrientationSensor.xml" path="//Member[@MemberName='Stop']/Docs" />
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

		internal static void OnChanged(OrientationSensorData reading) =>
			OnChanged(new OrientationSensorChangedEventArgs(reading));

		internal static void OnChanged(OrientationSensorChangedEventArgs e)
		{
			if (useSyncContext)
				MainThread.BeginInvokeOnMainThread(() => ReadingChanged?.Invoke(null, e));
			else
				ReadingChanged?.Invoke(null, e);
		}
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/OrientationSensorChangedEventArgs.xml" path="Type[@FullName='Microsoft.Maui.Essentials.OrientationSensorChangedEventArgs']/Docs" />
	public class OrientationSensorChangedEventArgs : EventArgs
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/OrientationSensorChangedEventArgs.xml" path="//Member[@MemberName='.ctor']/Docs" />
		public OrientationSensorChangedEventArgs(OrientationSensorData reading) =>
			Reading = reading;

		/// <include file="../../docs/Microsoft.Maui.Essentials/OrientationSensorChangedEventArgs.xml" path="//Member[@MemberName='Reading']/Docs" />
		public OrientationSensorData Reading { get; }
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/OrientationSensorData.xml" path="Type[@FullName='Microsoft.Maui.Essentials.OrientationSensorData']/Docs" />
	public readonly struct OrientationSensorData : IEquatable<OrientationSensorData>
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/OrientationSensorData.xml" path="//Member[@MemberName='.ctor'][0]/Docs" />
		public OrientationSensorData(double x, double y, double z, double w)
			: this((float)x, (float)y, (float)z, (float)w)
		{
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/OrientationSensorData.xml" path="//Member[@MemberName='.ctor'][1]/Docs" />
		public OrientationSensorData(float x, float y, float z, float w) =>
			Orientation = new Quaternion(x, y, z, w);

		/// <include file="../../docs/Microsoft.Maui.Essentials/OrientationSensorData.xml" path="//Member[@MemberName='Orientation']/Docs" />
		public Quaternion Orientation { get; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/OrientationSensorData.xml" path="//Member[@MemberName='Equals'][0]/Docs" />
		public override bool Equals(object obj) =>
			(obj is OrientationSensorData data) && Equals(data);

		/// <include file="../../docs/Microsoft.Maui.Essentials/OrientationSensorData.xml" path="//Member[@MemberName='Equals'][1]/Docs" />
		public bool Equals(OrientationSensorData other) =>
			Orientation.Equals(other.Orientation);

		public static bool operator ==(OrientationSensorData left, OrientationSensorData right) =>
			left.Equals(right);

		public static bool operator !=(OrientationSensorData left, OrientationSensorData right) =>
		   !left.Equals(right);

		/// <include file="../../docs/Microsoft.Maui.Essentials/OrientationSensorData.xml" path="//Member[@MemberName='GetHashCode']/Docs" />
		public override int GetHashCode() =>
			Orientation.GetHashCode();

		/// <include file="../../docs/Microsoft.Maui.Essentials/OrientationSensorData.xml" path="//Member[@MemberName='ToString']/Docs" />
		public override string ToString() =>
			$"{nameof(Orientation.X)}: {Orientation.X}, " +
			$"{nameof(Orientation.Y)}: {Orientation.Y}, " +
			$"{nameof(Orientation.Z)}: {Orientation.Z}, " +
			$"{nameof(Orientation.W)}: {Orientation.W}";
	}
}
