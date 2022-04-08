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
		static Sensor? _accelerometer;

		static SensorManager? SensorManager =>
			_sensorManager ??= Application.Context.GetSystemService(Context.SensorService) as SensorManager;

		static Sensor? Sensor =>
			_accelerometer ??= SensorManager?.GetDefaultSensor(SensorType.Accelerometer);

		public bool IsSupported => Sensor is not null;

		AccelerometerListener? _listener;

		void PlatformStart(SensorSpeed sensorSpeed)
		{
			_listener = new AccelerometerListener(OnChanged);

			var delay = sensorSpeed.ToPlatform();
			SensorManager!.RegisterListener(_listener, Sensor, delay);
		}

		void PlatformStop()
		{
			SensorManager!.UnregisterListener(_listener, Sensor);

			_listener!.Dispose();
			_listener = null;
		}
	}

	class AccelerometerListener : Java.Lang.Object, ISensorEventListener
	{
		// acceleration due to gravity
		const double gravity = 9.81;

		public AccelerometerListener(Action<AccelerometerData> changeHandler)
		{
			ChangeHandler = changeHandler;
		}

		public readonly Action<AccelerometerData> ChangeHandler;

		void ISensorEventListener.OnAccuracyChanged(Sensor? sensor, SensorStatus accuracy)
		{
		}

		void ISensorEventListener.OnSensorChanged(SensorEvent? e)
		{
			var values = e?.Values ?? Array.Empty<float>();
			if (values.Count < 3)
				return;

			var data = new AccelerometerData(values[0] / gravity, values[1] / gravity, values[2] / gravity);
			ChangeHandler?.Invoke(data);
		}
	}
}
