#nullable enable
using System;
using Microsoft.Maui.Devices.Sensors;

namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/Barometer.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Barometer']/Docs" />
	public static class Barometer
	{
		public static event EventHandler<BarometerChangedEventArgs> ReadingChanged
		{
			add => Current.ReadingChanged += value;
			remove => Current.ReadingChanged -= value;
		}

		public static bool IsSupported => Current.IsSupported;

		/// <include file="../../docs/Microsoft.Maui.Essentials/Barometer.xml" path="//Member[@MemberName='IsMonitoring']/Docs" />
		public static bool IsMonitoring
			=> Current.IsMonitoring;

		/// <include file="../../docs/Microsoft.Maui.Essentials/Barometer.xml" path="//Member[@MemberName='Start']/Docs" />
		public static void Start(SensorSpeed sensorSpeed)
			=> Current.Start(sensorSpeed);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Barometer.xml" path="//Member[@MemberName='Stop']/Docs" />
		public static void Stop()
			=> Current.Stop();

		static IBarometer Current => Devices.Sensors.Barometer.Default;
	}
}
