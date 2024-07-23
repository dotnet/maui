#nullable enable
using System;
using Android.App;
using Android.Content;
using Android.Hardware;

namespace Microsoft.Maui.Devices.Sensors
{
	partial class OrientationSensorImplementation : IOrientationSensor
	{
		static Sensor? orientationSensor;

		static SensorManager? SensorManager =>
			Application.Context.GetSystemService(Context.SensorService) as SensorManager;

		static Sensor? Sensor =>
			orientationSensor ??= SensorManager?.GetDefaultSensor(SensorType.RotationVector);

		bool PlatformIsSupported =>
			Sensor is not null;

		OrientationSensorListener? listener;

		void PlatformStart(SensorSpeed sensorSpeed)
		{
			var delay = sensorSpeed.ToPlatform();

			listener = new OrientationSensorListener(RaiseReadingChanged);
			SensorManager!.RegisterListener(listener, Sensor, delay);
		}

		void PlatformStop()
		{
			if (listener == null || Sensor == null)
				return;

			SensorManager!.UnregisterListener(listener, Sensor);
			listener.Dispose();
			listener = null;
		}
	}

	class OrientationSensorListener : Java.Lang.Object, ISensorEventListener
	{
		internal OrientationSensorListener(Action<OrientationSensorData> callback)
		{
			Callback = callback;
		}

		readonly Action<OrientationSensorData> Callback;

		void ISensorEventListener.OnAccuracyChanged(Sensor? sensor, SensorStatus accuracy)
		{
		}

		void ISensorEventListener.OnSensorChanged(SensorEvent? e)
		{
			var values = e?.Values ?? Array.Empty<float>();
			if (values.Count < 3)
				return;

			OrientationSensorData? data;

			// Docs: https://developer.android.com/reference/android/hardware/SensorEvent#sensor.type_rotation_vector:
			// values[3], originally optional, will always be present from SDK Level 18 onwards. values[4] is a new value that has been added in SDK Level 18.

			if (values.Count < 4)
				data = new OrientationSensorData(values[0], values[1], values[2], -1);
			else
				data = new OrientationSensorData(values[0], values[1], values[2], values[3]);

			Callback?.Invoke(data.Value);
		}
	}
}
