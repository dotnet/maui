#nullable enable
using System;
using System.Numerics;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Devices.Sensors
{
	public interface IMagnetometer
	{
		bool IsSupported { get; }

		bool IsMonitoring { get; }

		void Start(SensorSpeed sensorSpeed);

		void Stop();

		event EventHandler<MagnetometerChangedEventArgs> ReadingChanged;
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/Magnetometer.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Magnetometer']/Docs/*" />
	public static partial class Magnetometer
	{
		public static event EventHandler<MagnetometerChangedEventArgs> ReadingChanged
		{
			add => Current.ReadingChanged += value;
			remove => Current.ReadingChanged -= value;
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/Magnetometer.xml" path="//Member[@MemberName='IsMonitoring']/Docs/*" />
		public static bool IsMonitoring
			=> Current.IsMonitoring;

		public static bool IsSupported
			=> Current.IsSupported;

		/// <include file="../../docs/Microsoft.Maui.Essentials/Magnetometer.xml" path="//Member[@MemberName='Start']/Docs/*" />
		public static void Start(SensorSpeed sensorSpeed)
			=> Current.Start(sensorSpeed);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Magnetometer.xml" path="//Member[@MemberName='Stop'][1]/Docs/*" />
		public static void Stop()
			=> Current.Stop();

		static IMagnetometer Current => Devices.Sensors.Magnetometer.Default;

		static IMagnetometer? defaultImplementation;

		public static IMagnetometer Default =>
			defaultImplementation ??= new MagnetometerImplementation();

		internal static void SetDefault(IMagnetometer? implementation) =>
			defaultImplementation = implementation;
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/MagnetometerChangedEventArgs.xml" path="Type[@FullName='Microsoft.Maui.Essentials.MagnetometerChangedEventArgs']/Docs/*" />
	public class MagnetometerChangedEventArgs : EventArgs
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/MagnetometerChangedEventArgs.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public MagnetometerChangedEventArgs(MagnetometerData reading) =>
			Reading = reading;

		/// <include file="../../docs/Microsoft.Maui.Essentials/MagnetometerChangedEventArgs.xml" path="//Member[@MemberName='Reading']/Docs/*" />
		public MagnetometerData Reading { get; }
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/MagnetometerData.xml" path="Type[@FullName='Microsoft.Maui.Essentials.MagnetometerData']/Docs/*" />
	public readonly struct MagnetometerData : IEquatable<MagnetometerData>
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/MagnetometerData.xml" path="//Member[@MemberName='.ctor'][1]/Docs/*" />
		public MagnetometerData(double x, double y, double z)
			: this((float)x, (float)y, (float)z)
		{
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/MagnetometerData.xml" path="//Member[@MemberName='.ctor'][2]/Docs/*" />
		public MagnetometerData(float x, float y, float z) =>
			MagneticField = new Vector3(x, y, z);

		/// <include file="../../docs/Microsoft.Maui.Essentials/MagnetometerData.xml" path="//Member[@MemberName='MagneticField']/Docs/*" />
		public Vector3 MagneticField { get; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/MagnetometerData.xml" path="//Member[@MemberName='Equals'][1]/Docs/*" />
		public override bool Equals(object? obj) =>
			(obj is MagnetometerData data) && Equals(data);

		/// <include file="../../docs/Microsoft.Maui.Essentials/MagnetometerData.xml" path="//Member[@MemberName='Equals'][2]/Docs/*" />
		public bool Equals(MagnetometerData other) =>
			MagneticField.Equals(other.MagneticField);

		public static bool operator ==(MagnetometerData left, MagnetometerData right) =>
			left.Equals(right);

		public static bool operator !=(MagnetometerData left, MagnetometerData right) =>
		   !left.Equals(right);

		/// <include file="../../docs/Microsoft.Maui.Essentials/MagnetometerData.xml" path="//Member[@MemberName='GetHashCode']/Docs/*" />
		public override int GetHashCode() =>
			MagneticField.GetHashCode();

		/// <include file="../../docs/Microsoft.Maui.Essentials/MagnetometerData.xml" path="//Member[@MemberName='ToString']/Docs/*" />
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
