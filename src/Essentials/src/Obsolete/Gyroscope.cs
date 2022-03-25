#nullable enable
using System;
using Microsoft.Maui.Devices.Sensors;

namespace Microsoft.Maui.Devices.Sensors
{
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

		/// <include file="../../docs/Microsoft.Maui.Essentials/Gyroscope.xml" path="//Member[@MemberName='Start']/Docs" />
		public static void Start(SensorSpeed sensorSpeed)
			=> Current.Start(sensorSpeed);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Gyroscope.xml" path="//Member[@MemberName='Stop'][1]/Docs" />
		public static void Stop()
			=> Current.Stop();

		static IGyroscope Current => Devices.Sensors.Gyroscope.Default;
	}
}
