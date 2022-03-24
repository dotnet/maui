#nullable enable
using System;
using Microsoft.Maui.Devices.Sensors;

namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/OrientationSensor.xml" path="Type[@FullName='Microsoft.Maui.Essentials.OrientationSensor']/Docs" />
	public static class OrientationSensor
	{
		public static event EventHandler<OrientationSensorChangedEventArgs> ReadingChanged
		{
			add => Current.ReadingChanged += value;
			remove => Current.ReadingChanged -= value;
		}

		public static bool IsSupported
			=> Current.IsSupported;

		/// <include file="../../docs/Microsoft.Maui.Essentials/OrientationSensor.xml" path="//Member[@MemberName='IsMonitoring']/Docs" />
		public static bool IsMonitoring { get; private set; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/OrientationSensor.xml" path="//Member[@MemberName='Start']/Docs" />
		public static void Start(SensorSpeed sensorSpeed)
			=> Current.Start(sensorSpeed);

		/// <include file="../../docs/Microsoft.Maui.Essentials/OrientationSensor.xml" path="//Member[@MemberName='Stop'][1]/Docs" />
		public static void Stop()
			=> Current.Stop();

		static IOrientationSensor Current => Devices.Sensors.OrientationSensor.Default;
	}
}
