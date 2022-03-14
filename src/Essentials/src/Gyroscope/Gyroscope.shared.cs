using System;
using System.Numerics;
using System.ComponentModel;
using Microsoft.Maui.Essentials;
using Microsoft.Maui.Essentials.Implementations;

namespace Microsoft.Maui.Essentials
{
	public interface IGyroscope
	{
		bool IsSupported { get; }

		bool IsMonitoring { get; }

		SensorSpeed SensorSpeed { get; }

		void Start(SensorSpeed sensorSpeed);

		void Stop();

		event EventHandler<GyroscopeChangedEventArgs> ReadingChanged;
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/Gyroscope.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Gyroscope']/Docs" />
	public static partial class Gyroscope
	{
		public static event EventHandler<GyroscopeChangedEventArgs> ReadingChanged
		{
			add => Current.ReadingChanged += value;
			remove => Current.ReadingChanged -= value;
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/Gyroscope.xml" path="//Member[@MemberName='IsMonitoring']/Docs" />
		public static bool IsMonitoring
			=> Current.IsMonitoring;

		public static bool IsSupported 
			=> Current.IsSupported;

		public static SensorSpeed SensorSpeed
			=> Current.SensorSpeed;

		/// <include file="../../docs/Microsoft.Maui.Essentials/Gyroscope.xml" path="//Member[@MemberName='Start']/Docs" />
		public static void Start(SensorSpeed sensorSpeed)
			=> Current.Start(sensorSpeed);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Gyroscope.xml" path="//Member[@MemberName='Stop'][1]/Docs" />
		public static void Stop()
			=> Current.Stop();

#nullable enable
		static IGyroscope? currentImplementation;
#nullable disable

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static IGyroscope Current =>
			currentImplementation ??= new GyroscopeImplementation();

		[EditorBrowsable(EditorBrowsableState.Never)]
#nullable enable
		public static void SetCurrent(IGyroscope? implementation) =>
			currentImplementation = implementation;
#nullable disable
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
		/// <include file="../../docs/Microsoft.Maui.Essentials/GyroscopeData.xml" path="//Member[@MemberName='.ctor'][1]/Docs" />
		public GyroscopeData(double x, double y, double z)
			: this((float)x, (float)y, (float)z)
		{
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/GyroscopeData.xml" path="//Member[@MemberName='.ctor'][2]/Docs" />
		public GyroscopeData(float x, float y, float z) =>
			AngularVelocity = new Vector3(x, y, z);

		/// <include file="../../docs/Microsoft.Maui.Essentials/GyroscopeData.xml" path="//Member[@MemberName='AngularVelocity']/Docs" />
		public Vector3 AngularVelocity { get; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/GyroscopeData.xml" path="//Member[@MemberName='Equals'][1]/Docs" />
		public override bool Equals(object obj) =>
			(obj is GyroscopeData data) && Equals(data);

		/// <include file="../../docs/Microsoft.Maui.Essentials/GyroscopeData.xml" path="//Member[@MemberName='Equals'][2]/Docs" />
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

namespace Microsoft.Maui.Essentials.Implementations
{
	public partial class GyroscopeImplementation : IGyroscope
	{
		bool UseSyncContext => SensorSpeed == SensorSpeed.Default || SensorSpeed == SensorSpeed.UI;

		public SensorSpeed SensorSpeed { get; private set; } = SensorSpeed.Default;

		public event EventHandler<GyroscopeChangedEventArgs> ReadingChanged;

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

		/// <include file="../../docs/Microsoft.Maui.Essentials/Gyroscope.xml" path="//Member[@MemberName='Stop'][2]/Docs" />
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
