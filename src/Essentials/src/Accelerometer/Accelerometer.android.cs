#nullable enable
using System;
using Android.App;
using Android.Content;
using Android.Hardware;

namespace Microsoft.Maui.Devices.Sensors
{
	partial class AccelerometerImplementation
	{
		static SensorManager? _sensorManager;

		static SensorManager? SensorManager =>
			_sensorManager ??= Application.Context.GetSystemService(Context.SensorService) as SensorManager;

		public bool IsSupported =>
			SensorManager?.GetDefaultSensor(SensorType.Accelerometer) != null;

		AccelerometerListener? listener;
		Sensor? accelerometer;

		void PlatformStart(SensorSpeed sensorSpeed)
		{
			if (SensorManager is null)
				return;

			accelerometer = SensorManager.GetDefaultSensor(SensorType.Accelerometer);
			if (accelerometer is not null)
			{
				listener = new AccelerometerListener(this);

				var delay = sensorSpeed.ToPlatform();
				SensorManager.RegisterListener(listener, accelerometer, delay);
			}
		}

		void PlatformStop()
		{
			if (listener == null || accelerometer == null)
				return;

			SensorManager?.UnregisterListener(listener, accelerometer);

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

		void ISensorEventListener.OnAccuracyChanged(Sensor? sensor, SensorStatus accuracy)
		{
		}

		void ISensorEventListener.OnSensorChanged(SensorEvent? e)
		{
			var values = e?.Values ?? Array.Empty<float>();
			if (values.Count < 3)
				return;

			var data = new AccelerometerData(values[0] / gravity, values[1] / gravity, values[2] / gravity);
			_accelerometer.OnChanged(data);
		}
	}
}
