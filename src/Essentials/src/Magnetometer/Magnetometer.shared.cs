using System;
using System.Numerics;

namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/Magnetometer.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Magnetometer']/Docs" />
	public static partial class Magnetometer
	{
		static bool useSyncContext;

		public static event EventHandler<MagnetometerChangedEventArgs> ReadingChanged;

		/// <include file="../../docs/Microsoft.Maui.Essentials/Magnetometer.xml" path="//Member[@MemberName='IsMonitoring']/Docs" />
		public static bool IsMonitoring { get; private set; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/Magnetometer.xml" path="//Member[@MemberName='Start']/Docs" />
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

		/// <include file="../../docs/Microsoft.Maui.Essentials/Magnetometer.xml" path="//Member[@MemberName='Stop']/Docs" />
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

	/// <include file="../../docs/Microsoft.Maui.Essentials/MagnetometerChangedEventArgs.xml" path="Type[@FullName='Microsoft.Maui.Essentials.MagnetometerChangedEventArgs']/Docs" />
	public class MagnetometerChangedEventArgs : EventArgs
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/MagnetometerChangedEventArgs.xml" path="//Member[@MemberName='.ctor']/Docs" />
		public MagnetometerChangedEventArgs(MagnetometerData reading) =>
			Reading = reading;

		/// <include file="../../docs/Microsoft.Maui.Essentials/MagnetometerChangedEventArgs.xml" path="//Member[@MemberName='Reading']/Docs" />
		public MagnetometerData Reading { get; }
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/MagnetometerData.xml" path="Type[@FullName='Microsoft.Maui.Essentials.MagnetometerData']/Docs" />
	public readonly struct MagnetometerData : IEquatable<MagnetometerData>
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/MagnetometerData.xml" path="//Member[@MemberName='.ctor'][0]/Docs" />
		public MagnetometerData(double x, double y, double z)
			: this((float)x, (float)y, (float)z)
		{
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/MagnetometerData.xml" path="//Member[@MemberName='.ctor'][1]/Docs" />
		public MagnetometerData(float x, float y, float z) =>
			MagneticField = new Vector3(x, y, z);

		/// <include file="../../docs/Microsoft.Maui.Essentials/MagnetometerData.xml" path="//Member[@MemberName='MagneticField']/Docs" />
		public Vector3 MagneticField { get; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/MagnetometerData.xml" path="//Member[@MemberName='Equals'][0]/Docs" />
		public override bool Equals(object obj) =>
			(obj is MagnetometerData data) && Equals(data);

		/// <include file="../../docs/Microsoft.Maui.Essentials/MagnetometerData.xml" path="//Member[@MemberName='Equals'][1]/Docs" />
		public bool Equals(MagnetometerData other) =>
			MagneticField.Equals(other.MagneticField);

		public static bool operator ==(MagnetometerData left, MagnetometerData right) =>
			left.Equals(right);

		public static bool operator !=(MagnetometerData left, MagnetometerData right) =>
		   !left.Equals(right);

		/// <include file="../../docs/Microsoft.Maui.Essentials/MagnetometerData.xml" path="//Member[@MemberName='GetHashCode']/Docs" />
		public override int GetHashCode() =>
			MagneticField.GetHashCode();

		/// <include file="../../docs/Microsoft.Maui.Essentials/MagnetometerData.xml" path="//Member[@MemberName='ToString']/Docs" />
		public override string ToString() =>
			$"{nameof(MagneticField.X)}: {MagneticField.X}, " +
			$"{nameof(MagneticField.Y)}: {MagneticField.Y}, " +
			$"{nameof(MagneticField.Z)}: {MagneticField.Z}";
	}
}
