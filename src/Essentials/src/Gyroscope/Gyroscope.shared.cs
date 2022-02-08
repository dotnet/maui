using System;
using System.Numerics;

namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/Gyroscope.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Gyroscope']/Docs" />
	public static partial class Gyroscope
	{
		static bool useSyncContext;

		public static event EventHandler<GyroscopeChangedEventArgs> ReadingChanged;

		/// <include file="../../docs/Microsoft.Maui.Essentials/Gyroscope.xml" path="//Member[@MemberName='IsMonitoring']/Docs" />
		public static bool IsMonitoring { get; private set; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/Gyroscope.xml" path="//Member[@MemberName='Start']/Docs" />
		public static void Start(SensorSpeed sensorSpeed)
		{
			if (!IsSupported)
				throw new FeatureNotSupportedException();

			if (IsMonitoring)
				throw new InvalidOperationException("Gyroscope has already been started.");

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

		/// <include file="../../docs/Microsoft.Maui.Essentials/Gyroscope.xml" path="//Member[@MemberName='Stop']/Docs" />
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

		internal static void OnChanged(GyroscopeData reading) =>
			OnChanged(new GyroscopeChangedEventArgs(reading));

		internal static void OnChanged(GyroscopeChangedEventArgs e)
		{
			if (useSyncContext)
				MainThread.BeginInvokeOnMainThread(() => ReadingChanged?.Invoke(null, e));
			else
				ReadingChanged?.Invoke(null, e);
		}
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/GyroscopeChangedEventArgs.xml" path="Type[@FullName='Microsoft.Maui.Essentials.GyroscopeChangedEventArgs']/Docs" />
	public class GyroscopeChangedEventArgs : EventArgs
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/GyroscopeChangedEventArgs.xml" path="//Member[@MemberName='.ctor']/Docs" />
		public GyroscopeChangedEventArgs(GyroscopeData reading) =>
			Reading = reading;

		/// <include file="../../docs/Microsoft.Maui.Essentials/GyroscopeChangedEventArgs.xml" path="//Member[@MemberName='Reading']/Docs" />
		public GyroscopeData Reading { get; }
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/GyroscopeData.xml" path="Type[@FullName='Microsoft.Maui.Essentials.GyroscopeData']/Docs" />
	public readonly struct GyroscopeData : IEquatable<GyroscopeData>
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/GyroscopeData.xml" path="//Member[@MemberName='.ctor'][0]/Docs" />
		public GyroscopeData(double x, double y, double z)
			: this((float)x, (float)y, (float)z)
		{
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/GyroscopeData.xml" path="//Member[@MemberName='.ctor'][1]/Docs" />
		public GyroscopeData(float x, float y, float z) =>
			AngularVelocity = new Vector3(x, y, z);

		/// <include file="../../docs/Microsoft.Maui.Essentials/GyroscopeData.xml" path="//Member[@MemberName='AngularVelocity']/Docs" />
		public Vector3 AngularVelocity { get; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/GyroscopeData.xml" path="//Member[@MemberName='Equals'][0]/Docs" />
		public override bool Equals(object obj) =>
			(obj is GyroscopeData data) && Equals(data);

		/// <include file="../../docs/Microsoft.Maui.Essentials/GyroscopeData.xml" path="//Member[@MemberName='Equals'][1]/Docs" />
		public bool Equals(GyroscopeData other) =>
			AngularVelocity.Equals(other.AngularVelocity);

		public static bool operator ==(GyroscopeData left, GyroscopeData right) =>
		  left.Equals(right);

		public static bool operator !=(GyroscopeData left, GyroscopeData right) =>
		   !left.Equals(right);

		/// <include file="../../docs/Microsoft.Maui.Essentials/GyroscopeData.xml" path="//Member[@MemberName='GetHashCode']/Docs" />
		public override int GetHashCode() =>
			AngularVelocity.GetHashCode();

		/// <include file="../../docs/Microsoft.Maui.Essentials/GyroscopeData.xml" path="//Member[@MemberName='ToString']/Docs" />
		public override string ToString() =>
			$"{nameof(AngularVelocity.X)}: {AngularVelocity.X}, " +
			$"{nameof(AngularVelocity.Y)}: {AngularVelocity.Y}, " +
			$"{nameof(AngularVelocity.Z)}: {AngularVelocity.Z}";
	}
}
