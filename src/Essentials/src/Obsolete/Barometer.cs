#nullable enable
using System;

namespace Microsoft.Maui.Devices.Sensors
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/Barometer.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Barometer']/Docs" />
	public static partial class Barometer
	{
		[Obsolete($"Use {nameof(Barometer)}.{nameof(Default)} instead.", true)]
		public static event EventHandler<BarometerChangedEventArgs> ReadingChanged
		{
			add => Default.ReadingChanged += value;
			remove => Default.ReadingChanged -= value;
		}

		[Obsolete($"Use {nameof(Barometer)}.{nameof(Default)} instead.", true)]
		public static bool IsSupported => Default.IsSupported;

		/// <include file="../../docs/Microsoft.Maui.Essentials/Barometer.xml" path="//Member[@MemberName='IsMonitoring']/Docs" />
		[Obsolete($"Use {nameof(Barometer)}.{nameof(Default)} instead.", true)]
		public static bool IsMonitoring
			=> Default.IsMonitoring;

		/// <include file="../../docs/Microsoft.Maui.Essentials/Barometer.xml" path="//Member[@MemberName='Start']/Docs" />
		[Obsolete($"Use {nameof(Barometer)}.{nameof(Default)} instead.", true)]
		public static void Start(SensorSpeed sensorSpeed)
			=> Default.Start(sensorSpeed);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Barometer.xml" path="//Member[@MemberName='Stop']/Docs" />
		[Obsolete($"Use {nameof(Barometer)}.{nameof(Default)} instead.", true)]
		public static void Stop()
			=> Default.Stop();
	}
}
