using System;
using Windows.Devices.Sensors;
using WinBarometer = Windows.Devices.Sensors.Barometer;

namespace Microsoft.Maui.Essentials.Implementations
{
	public class BarometerImplementation : IBarometer
	{
		static WinBarometer sensor;

		static WinBarometer DefaultBarometer => WinBarometer.GetDefault();

		bool UseSyncContext => SensorSpeed == SensorSpeed.Default || SensorSpeed == SensorSpeed.UI;

		public event EventHandler<BarometerChangedEventArgs> ReadingChanged;

		public bool IsMonitoring { get; private set; }

		public SensorSpeed SensorSpeed { get; private set; } = SensorSpeed.Default;

		public bool IsSupported =>
			DefaultBarometer != null;

		public void Start(SensorSpeed sensorSpeed)
		{
			if (!IsSupported)
				throw new FeatureNotSupportedException();

			if (IsMonitoring)
				throw new InvalidOperationException("Barometer has already been started.");

			IsMonitoring = true;
			SensorSpeed = sensorSpeed;

			try
			{
				sensor = DefaultBarometer;

				var interval = sensorSpeed.ToPlatform();
				sensor.ReportInterval = sensor.MinimumReportInterval >= interval ? sensor.MinimumReportInterval : interval;

				sensor.ReadingChanged += BarometerReportedInterval;
			}
			catch
			{
				IsMonitoring = false;
				throw;
			}
		}

		internal void BarometerReportedInterval(object sender, BarometerReadingChangedEventArgs e)
		{
			var data = new BarometerData(e.Reading.StationPressureInHectopascals);

			if (UseSyncContext)
				MainThread.BeginInvokeOnMainThread(() => ReadingChanged?.Invoke(this, new BarometerChangedEventArgs(data)));
			else
				ReadingChanged?.Invoke(this, new BarometerChangedEventArgs(data));
		}

		public void Stop()
		{
			if (!IsSupported)
				throw new FeatureNotSupportedException();

			if (!IsMonitoring)
				return;

			IsMonitoring = false;

			if (sensor == null)
				return;

			try
			{
				sensor.ReadingChanged -= BarometerReportedInterval;
				sensor.ReportInterval = 0;
				sensor = null;
			}
			catch
			{
				IsMonitoring = true;
				throw;
			}
		}
	}
}
