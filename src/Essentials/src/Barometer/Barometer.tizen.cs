using System;
using Tizen.Sensor;
using TizenBarometerSensor = Tizen.Sensor.PressureSensor;

namespace Microsoft.Maui.Essentials.Implementations
{
	public class BarometerImplementation : IBarometer
	{
		bool UseSyncContext => SensorSpeed == SensorSpeed.Default || SensorSpeed == SensorSpeed.UI;

		public SensorSpeed SensorSpeed { get; private set; } = SensorSpeed.Default;

		public event EventHandler<BarometerChangedEventArgs> ReadingChanged;

		static TizenBarometerSensor DefaultSensor
			=> (TizenBarometerSensor)Platform.GetDefaultSensor(SensorType.Barometer);

		public bool IsSupported
			=> TizenBarometerSensor.IsSupported;

		public bool IsMonitoring { get; private set; }

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
				DefaultSensor.Interval = sensorSpeed.ToPlatform();
				DefaultSensor.DataUpdated += DataUpdated;
				DefaultSensor.Start();
			}
			catch
			{
				IsMonitoring = false;
				throw;
			}
		}

		void DataUpdated(object sender, PressureSensorDataUpdatedEventArgs e)
		{
			var data = new BarometerData(e.Pressure);

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

			try
			{
				DefaultSensor.DataUpdated -= DataUpdated;
				DefaultSensor.Stop();
			}
			catch
			{
				IsMonitoring = true;
				throw;
			}
		}
	}
}
