using Android.Hardware;
using Android.Runtime;

namespace Microsoft.Maui.Essentials
{
	public partial class AccelerometerImpl
	{
		public bool IsSupported =>
			Platform.SensorManager?.GetDefaultSensor(SensorType.Accelerometer) != null;

		AccelerometerListener listener;
		Sensor accelerometer;

		private void PlatformStart(SensorSpeed sensorSpeed)
		{
			var delay = sensorSpeed.ToPlatform();
			listener = new AccelerometerListener(this);
			accelerometer = Platform.SensorManager.GetDefaultSensor(SensorType.Accelerometer);
			Platform.SensorManager.RegisterListener(listener, accelerometer, delay);
		}

		private void PlatformStop()
		{
			if (listener == null || accelerometer == null)
				return;

			Platform.SensorManager.UnregisterListener(listener, accelerometer);
			listener.Dispose();
			listener = null;
		}
	}

	class AccelerometerListener : Java.Lang.Object, ISensorEventListener
	{
		// acceleration due to gravity
		const double gravity = 9.81;

		private AccelerometerImpl accelerometer;

		internal AccelerometerListener(AccelerometerImpl accelerometer)
		{
			this.accelerometer = accelerometer;
		}

		void ISensorEventListener.OnAccuracyChanged(Sensor sensor, [GeneratedEnum] SensorStatus accuracy)
		{
		}

		void ISensorEventListener.OnSensorChanged(SensorEvent e)
		{
			if ((e?.Values?.Count ?? 0) < 3)
				return;

			var data = new AccelerometerData(e.Values[0] / gravity, e.Values[1] / gravity, e.Values[2] / gravity);
			accelerometer.OnChanged(data);
		}
	}
}
