#nullable enable
using System;
using Microsoft.Maui.Devices.Sensors;

namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/Compass.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Compass']/Docs" />
	public static partial class Compass
	{
		public static event EventHandler<CompassChangedEventArgs> ReadingChanged
		{
			add => Current.ReadingChanged += value;
			remove => Current.ReadingChanged -= value;
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/Compass.xml" path="//Member[@MemberName='IsMonitoring']/Docs" />
		public static bool IsSupported
			=> Current.IsSupported;

		public static bool IsMonitoring
			=> Current.IsMonitoring;

		/// <include file="../../docs/Microsoft.Maui.Essentials/Compass.xml" path="//Member[@MemberName='Start'][1]/Docs" />
		public static void Start(SensorSpeed sensorSpeed)
			=> Start(sensorSpeed, true);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Compass.xml" path="//Member[@MemberName='Start'][2]/Docs" />
		public static void Start(SensorSpeed sensorSpeed, bool applyLowPassFilter)
			=> Current.Start(sensorSpeed, applyLowPassFilter);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Compass.xml" path="//Member[@MemberName='Stop'][1]/Docs" />
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
	}
}
