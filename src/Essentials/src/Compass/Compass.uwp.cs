using Windows.Devices.Sensors;

using WindowsCompass = Windows.Devices.Sensors.Compass;

namespace Microsoft.Maui.Essentials.Implementations
{
	public partial class CompassImplementation : ICompass
	{
		// Magic numbers from https://docs.microsoft.com/en-us/uwp/api/windows.devices.sensors.compass.reportinterval#Windows_Devices_Sensors_Compass_ReportInterval
		internal const uint FastestInterval = 8;
		internal const uint GameInterval = 22;
		internal const uint NormalInterval = 33;

		// keep around a reference so we can stop this same instance
		static WindowsCompass sensor;

		internal static WindowsCompass DefaultCompass =>
			WindowsCompass.GetDefault();

		public bool IsSupported =>
			DefaultCompass != null;

		public bool IsMonitoring { get; set; }

		public void Start(SensorSpeed sensorSpeed)
			=> Start(sensorSpeed, false);

		public void Start(SensorSpeed sensorSpeed, bool applyLowPassFilter)
		{
			sensor = DefaultCompass;

			var interval = NormalInterval;
			switch (sensorSpeed)
			{
				case SensorSpeed.Fastest:
					interval = FastestInterval;
					break;
				case SensorSpeed.Game:
					interval = GameInterval;
					break;
			}

			sensor.ReportInterval = sensor.MinimumReportInterval >= interval ? sensor.MinimumReportInterval : interval;

			sensor.ReadingChanged += CompassReportedInterval;
		}

		void CompassReportedInterval(object sender, CompassReadingChangedEventArgs e)
		{
			var data = new CompassData(e.Reading.HeadingMagneticNorth);
			Compass.OnChanged(data);
		}

		public void Stop()
		{
			sensor.ReadingChanged -= CompassReportedInterval;
			sensor.ReportInterval = 0;
		}
	}
}
