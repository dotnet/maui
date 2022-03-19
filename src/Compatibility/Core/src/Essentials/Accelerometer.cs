#nullable enable
using System;
using Microsoft.Maui.Devices.Sensors;

namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/Accelerometer.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Accelerometer']/Docs" />
	public static partial class Accelerometer
	{
		public static event EventHandler<AccelerometerChangedEventArgs> ReadingChanged
		{
			add => Current.ReadingChanged += value;
			remove => Current.ReadingChanged -= value;
		}

		public static event EventHandler ShakeDetected
		{
			add => Current.ShakeDetected += value;
			remove => Current.ShakeDetected -= value;
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/Accelerometer.xml" path="//Member[@MemberName='IsMonitoring']/Docs" />
		public static bool IsMonitoring => Current.IsMonitoring;

		/// <include file="../../docs/Microsoft.Maui.Essentials/Accelerometer.xml" path="//Member[@MemberName='Start']/Docs" />
		public static void Start(SensorSpeed sensorSpeed) => Current.Start(sensorSpeed);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Accelerometer.xml" path="//Member[@MemberName='Stop']/Docs" />
		public static void Stop() => Current.Stop();

		static IAccelerometer Current => Devices.Sensors.Accelerometer.Default;
	}
}
