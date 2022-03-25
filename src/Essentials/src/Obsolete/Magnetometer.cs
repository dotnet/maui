#nullable enable
using System;
using Microsoft.Maui.Devices.Sensors;

namespace Microsoft.Maui.Devices.Sensors
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/Magnetometer.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Magnetometer']/Docs" />
	public static partial class Magnetometer
	{
		public static event EventHandler<MagnetometerChangedEventArgs> ReadingChanged
		{
			add => Current.ReadingChanged += value;
			remove => Current.ReadingChanged -= value;
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/Magnetometer.xml" path="//Member[@MemberName='IsMonitoring']/Docs" />
		public static bool IsMonitoring
			=> Current.IsMonitoring;

		public static bool IsSupported
			=> Current.IsSupported;

		/// <include file="../../docs/Microsoft.Maui.Essentials/Magnetometer.xml" path="//Member[@MemberName='Start']/Docs" />
		public static void Start(SensorSpeed sensorSpeed)
			=> Current.Start(sensorSpeed);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Magnetometer.xml" path="//Member[@MemberName='Stop'][1]/Docs" />
		public static void Stop()
			=> Current.Stop();

		static IMagnetometer Current => Devices.Sensors.Magnetometer.Default;
	}
}
