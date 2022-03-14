using System;
using System.Numerics;
using System.ComponentModel;
using Microsoft.Maui.Essentials;
using Microsoft.Maui.Essentials.Implementations;


namespace Microsoft.Maui.Essentials
{
	public interface IOrientationSensor
	{
		bool IsSupported { get; }

		bool IsMonitoring { get; }

		SensorSpeed SensorSpeed { get; }

		void Start(SensorSpeed sensorSpeed);

		void Stop();

		event EventHandler<OrientationSensorChangedEventArgs> ReadingChanged;
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/OrientationSensor.xml" path="Type[@FullName='Microsoft.Maui.Essentials.OrientationSensor']/Docs" />
	public static partial class OrientationSensor
	{
		public static event EventHandler<OrientationSensorChangedEventArgs> ReadingChanged
		{
			add => Current.ReadingChanged += value;
			remove => Current.ReadingChanged -= value;
		}

		public static bool IsSupported 
			=> Current.IsSupported;

		public static SensorSpeed SensorSpeed
			=> Current.SensorSpeed;

		/// <include file="../../docs/Microsoft.Maui.Essentials/OrientationSensor.xml" path="//Member[@MemberName='IsMonitoring']/Docs" />
		public static bool IsMonitoring { get; private set; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/OrientationSensor.xml" path="//Member[@MemberName='Start']/Docs" />
		public static void Start(SensorSpeed sensorSpeed)
			=> Current.Start(sensorSpeed);

		/// <include file="../../docs/Microsoft.Maui.Essentials/OrientationSensor.xml" path="//Member[@MemberName='Stop'][1]/Docs" />
		public static void Stop()
			=> Current.Stop();

#nullable enable
		static IOrientationSensor? currentImplementation;
#nullable disable

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static IOrientationSensor Current =>
			currentImplementation ??= new OrientationSensorImplementation();

		[EditorBrowsable(EditorBrowsableState.Never)]
#nullable enable
		public static void SetCurrent(IOrientationSensor? implementation) =>
			currentImplementation = implementation;
#nullable disable
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
		/// <include file="../../docs/Microsoft.Maui.Essentials/OrientationSensorData.xml" path="//Member[@MemberName='.ctor'][1]/Docs" />
		public OrientationSensorData(double x, double y, double z, double w)
			: this((float)x, (float)y, (float)z, (float)w)
		{
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/OrientationSensorData.xml" path="//Member[@MemberName='.ctor'][2]/Docs" />
		public OrientationSensorData(float x, float y, float z, float w) =>
			Orientation = new Quaternion(x, y, z, w);

		/// <include file="../../docs/Microsoft.Maui.Essentials/OrientationSensorData.xml" path="//Member[@MemberName='Orientation']/Docs" />
		public Quaternion Orientation { get; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/OrientationSensorData.xml" path="//Member[@MemberName='Equals'][1]/Docs" />
		public override bool Equals(object obj) =>
			(obj is OrientationSensorData data) && Equals(data);

		/// <include file="../../docs/Microsoft.Maui.Essentials/OrientationSensorData.xml" path="//Member[@MemberName='Equals'][2]/Docs" />
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

namespace Microsoft.Maui.Essentials.Implementations
{
	public partial class OrientationSensorImplementation : IOrientationSensor
	{
		bool UseSyncContext => SensorSpeed == SensorSpeed.Default || SensorSpeed == SensorSpeed.UI;

		public SensorSpeed SensorSpeed { get; private set; } = SensorSpeed.Default;

		public event EventHandler<OrientationSensorChangedEventArgs> ReadingChanged;

		public bool IsSupported
			=> PlatformIsSupported;

		public bool IsMonitoring { get; private set; }

		public void Start(SensorSpeed sensorSpeed)
		{
			if (!PlatformIsSupported)
				throw new FeatureNotSupportedException();

			if (IsMonitoring)
				throw new InvalidOperationException("Orientation sensor has already been started.");

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

		/// <include file="../../docs/Microsoft.Maui.Essentials/OrientationSensor.xml" path="//Member[@MemberName='Stop'][2]/Docs" />
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

		internal void RaiseReadingChanged(OrientationSensorData reading)
		{
			var args = new OrientationSensorChangedEventArgs(reading);
			
			if (UseSyncContext)
				MainThread.BeginInvokeOnMainThread(() => ReadingChanged?.Invoke(null, args));
			else
				ReadingChanged?.Invoke(null, args);
		}

	}
}
