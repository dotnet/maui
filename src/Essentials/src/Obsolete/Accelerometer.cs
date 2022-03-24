#nullable enable
using System;

namespace Microsoft.Maui.Devices.Sensors
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/Accelerometer.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Accelerometer']/Docs" />
	public static partial class Accelerometer
	{
		[Obsolete($"Use {nameof(Accelerometer)}.{nameof(Default)} instead.", true)]
		public static event EventHandler<AccelerometerChangedEventArgs> ReadingChanged
		{
			add => Default.ReadingChanged += value;
			remove => Default.ReadingChanged -= value;
		}

		[Obsolete($"Use {nameof(Accelerometer)}.{nameof(Default)} instead.", true)]
		public static event EventHandler ShakeDetected
		{
			add => Default.ShakeDetected += value;
			remove => Default.ShakeDetected -= value;
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/Accelerometer.xml" path="//Member[@MemberName='IsMonitoring']/Docs" />
		[Obsolete($"Use {nameof(Accelerometer)}.{nameof(Default)} instead.", true)]
		public static bool IsMonitoring => Default.IsMonitoring;

		/// <include file="../../docs/Microsoft.Maui.Essentials/Accelerometer.xml" path="//Member[@MemberName='Start']/Docs" />
		[Obsolete($"Use {nameof(Accelerometer)}.{nameof(Default)} instead.", true)]
		public static void Start(SensorSpeed sensorSpeed) => Default.Start(sensorSpeed);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Accelerometer.xml" path="//Member[@MemberName='Stop']/Docs" />
		[Obsolete($"Use {nameof(Accelerometer)}.{nameof(Default)} instead.", true)]
		public static void Stop() => Default.Stop();
	}
}
