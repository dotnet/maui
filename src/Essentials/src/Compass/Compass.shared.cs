#nullable enable
using System;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Devices.Sensors
{
	public interface ICompass
	{
		bool IsSupported { get; }

		bool IsMonitoring { get; }

		void Start(SensorSpeed sensorSpeed);

		void Start(SensorSpeed sensorSpeed, bool applyLowPassFilter);

		void Stop();

		event EventHandler<CompassChangedEventArgs> ReadingChanged;
	}

	public interface IPlatformCompass
	{
#if IOS || MACCATALYST
		bool ShouldDisplayHeadingCalibration { get; set; }
#endif
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/Compass.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Compass']/Docs/*" />
	public static class Compass
	{
		public static event EventHandler<CompassChangedEventArgs> ReadingChanged
		{
			add => Current.ReadingChanged += value;
			remove => Current.ReadingChanged -= value;
		}

		public static bool IsSupported
			=> Current.IsSupported;

		/// <include file="../../docs/Microsoft.Maui.Essentials/Compass.xml" path="//Member[@MemberName='IsMonitoring']/Docs/*" />
		public static bool IsMonitoring
			=> Current.IsMonitoring;

		/// <include file="../../docs/Microsoft.Maui.Essentials/Compass.xml" path="//Member[@MemberName='Start'][1]/Docs/*" />
		public static void Start(SensorSpeed sensorSpeed)
			=> Start(sensorSpeed, true);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Compass.xml" path="//Member[@MemberName='Start'][2]/Docs/*" />
		public static void Start(SensorSpeed sensorSpeed, bool applyLowPassFilter)
			=> Current.Start(sensorSpeed, applyLowPassFilter);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Compass.xml" path="//Member[@MemberName='Stop'][1]/Docs/*" />
		public static void Stop()
			=> Current.Stop();

#if IOS || MACCATALYST
		public static bool ShouldDisplayHeadingCalibration
		{
			get
			{
				if (Current is IPlatformCompass c)
					return c.ShouldDisplayHeadingCalibration;
				return false;
			}
			set
			{
				if (Current is IPlatformCompass c)
					c.ShouldDisplayHeadingCalibration = value;
			}
		}
#endif

		static ICompass Current => Devices.Sensors.Compass.Default;

		static ICompass? defaultImplementation;

		public static ICompass Default =>
			defaultImplementation ??= new CompassImplementation();

		internal static void SetDefault(ICompass? implementation) =>
			defaultImplementation = implementation;
	}

	public static class CompassExtensions
	{
#if IOS || MACCATALYST
		public static void SetShouldDisplayHeadingCalibration(this ICompass compass, bool shouldDisplay)
		{
			if (compass is IPlatformCompass platform)
			{
				platform.ShouldDisplayHeadingCalibration = shouldDisplay;
			}
		}
#endif
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/CompassChangedEventArgs.xml" path="Type[@FullName='Microsoft.Maui.Essentials.CompassChangedEventArgs']/Docs/*" />
	public class CompassChangedEventArgs : EventArgs
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/CompassChangedEventArgs.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public CompassChangedEventArgs(CompassData reading) =>
			Reading = reading;

		/// <include file="../../docs/Microsoft.Maui.Essentials/CompassChangedEventArgs.xml" path="//Member[@MemberName='Reading']/Docs/*" />
		public CompassData Reading { get; }
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/CompassData.xml" path="Type[@FullName='Microsoft.Maui.Essentials.CompassData']/Docs/*" />
	public readonly struct CompassData : IEquatable<CompassData>
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/CompassData.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public CompassData(double headingMagneticNorth) =>
			HeadingMagneticNorth = headingMagneticNorth;

		/// <include file="../../docs/Microsoft.Maui.Essentials/CompassData.xml" path="//Member[@MemberName='HeadingMagneticNorth']/Docs/*" />
		public double HeadingMagneticNorth { get; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/CompassData.xml" path="//Member[@MemberName='Equals'][1]/Docs/*" />
		public override bool Equals(object? obj) =>
			(obj is CompassData data) && Equals(data);

		/// <include file="../../docs/Microsoft.Maui.Essentials/CompassData.xml" path="//Member[@MemberName='Equals'][2]/Docs/*" />
		public bool Equals(CompassData other) =>
			HeadingMagneticNorth.Equals(other.HeadingMagneticNorth);

		public static bool operator ==(CompassData left, CompassData right) =>
			left.Equals(right);

		public static bool operator !=(CompassData left, CompassData right) =>
		   !left.Equals(right);

		/// <include file="../../docs/Microsoft.Maui.Essentials/CompassData.xml" path="//Member[@MemberName='GetHashCode']/Docs/*" />
		public override int GetHashCode() =>
			HeadingMagneticNorth.GetHashCode();

		/// <include file="../../docs/Microsoft.Maui.Essentials/CompassData.xml" path="//Member[@MemberName='ToString']/Docs/*" />
		public override string ToString() =>
			$"{nameof(HeadingMagneticNorth)}: {HeadingMagneticNorth}";
	}

	partial class CompassImplementation : ICompass
	{
		bool UseSyncContext => SensorSpeed == SensorSpeed.Default || SensorSpeed == SensorSpeed.UI;

		public event EventHandler<CompassChangedEventArgs>? ReadingChanged;

		public bool IsSupported
			=> PlatformIsSupported;

		public bool IsMonitoring { get; private set; }

		SensorSpeed SensorSpeed { get; set; }

		public void Start(SensorSpeed sensorSpeed) => Start(sensorSpeed, true);

		public void Start(SensorSpeed sensorSpeed, bool applyLowPassFilter)
		{
			if (!PlatformIsSupported)
				throw new FeatureNotSupportedException();

			if (IsMonitoring)
				throw new InvalidOperationException("Compass has already been started.");

			IsMonitoring = true;


			try
			{
				PlatformStart(sensorSpeed, applyLowPassFilter);
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

		internal void RaiseReadingChanged(CompassData data)
		{
			var args = new CompassChangedEventArgs(data);

			if (UseSyncContext)
				MainThread.BeginInvokeOnMainThread(() => ReadingChanged?.Invoke(null, args));
			else
				ReadingChanged?.Invoke(null, args);
		}
	}
}
