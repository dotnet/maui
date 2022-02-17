using Android.Hardware;
using Android.Runtime;

namespace Microsoft.Maui.Essentials.Implementations
{
	public partial class AccelerometerImplementation
	{
		public bool IsSupported =>
			Platform.SensorManager?.GetDefaultSensor(SensorType.Accelerometer) != null;

		AccelerometerListener listener;
		Sensor accelerometer;

		void PlatformStart(SensorSpeed sensorSpeed)
		{
			var delay = sensorSpeed.ToPlatform();
			listener = new AccelerometerListener(this);
			accelerometer = Platform.SensorManager.GetDefaultSensor(SensorType.Accelerometer);
			Platform.SensorManager.RegisterListener(listener, accelerometer, delay);
		}

		void PlatformStop()
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

		AccelerometerImplementation _accelerometer;

		internal AccelerometerListener(AccelerometerImplementation accelerometer)
		{
			_accelerometer = accelerometer;
		}

		void ISensorEventListener.OnAccuracyChanged(Sensor sensor, [GeneratedEnum] SensorStatus accuracy)
		{
		}

		void ISensorEventListener.OnSensorChanged(SensorEvent e)
		{
			if ((e?.Values?.Count ?? 0) < 3)
				return;

			var data = new AccelerometerData(e.Values[0] / gravity, e.Values[1] / gravity, e.Values[2] / gravity);
			_accelerometer.OnChanged(data);
		}
	}
}
