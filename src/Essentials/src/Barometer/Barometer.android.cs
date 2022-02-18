using System;
using Android.Hardware;
using Android.Runtime;

namespace Microsoft.Maui.Essentials.Implementations
{
	public class BarometerImplementation : IBarometer
	{
		public bool IsSupported
			=> DefaultBarometer != null;

		static Sensor DefaultBarometer => Platform.SensorManager?.GetDefaultSensor(SensorType.Pressure);

		static Sensor barometer;

		static BarometerListener listener;


		public bool IsMonitoring { get; set; }

		public void Start(SensorSpeed sensorSpeed)
		{
			listener = new BarometerListener();
			barometer = DefaultBarometer;
			Platform.SensorManager.RegisterListener(listener, barometer, sensorSpeed.ToPlatform());
		}

		public void Stop()
		{
			if (listener == null)
				return;

			Platform.SensorManager.UnregisterListener(listener, barometer);
			listener.Dispose();
			listener = null;
		}
	}

	class BarometerListener : Java.Lang.Object, ISensorEventListener, IDisposable
	{
		void ISensorEventListener.OnAccuracyChanged(Sensor sensor, [GeneratedEnum] SensorStatus accuracy)
		{
		}

		void ISensorEventListener.OnSensorChanged(SensorEvent e)
		{
			if ((e?.Values?.Count ?? 0) <= 0)
				return;

			Barometer.OnChanged(new BarometerData(e.Values[0]));
		}
	}
}
