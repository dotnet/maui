
#nullable enable
using System;
using Android.App;
using Android.Content;
using Android.Hardware;

namespace Microsoft.Maui.Devices.Sensors
{
	partial class GyroscopeImplementation : IGyroscope
	{
		static Sensor? gyroscope;

		static SensorManager? SensorManager =>
			Application.Context.GetSystemService(Context.SensorService) as SensorManager;

		static Sensor? Sensor =>
			gyroscope ??= SensorManager?.GetDefaultSensor(SensorType.Gyroscope);

		bool PlatformIsSupported =>
			Sensor is not null;

		GyroscopeListener? listener;

		void PlatformStart(SensorSpeed sensorSpeed)
		{
			var delay = sensorSpeed.ToPlatform();

			listener = new GyroscopeListener(RaiseReadingChanged);
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

	class GyroscopeListener : Java.Lang.Object, ISensorEventListener
	{
		internal GyroscopeListener(Action<GyroscopeData> callback)
		{
			Callback = callback;
		}

		readonly Action<GyroscopeData> Callback;

		void ISensorEventListener.OnAccuracyChanged(Sensor? sensor, SensorStatus accuracy)
		{
		}

		void ISensorEventListener.OnSensorChanged(SensorEvent? e)
		{
			var values = e?.Values ?? Array.Empty<float>();
			if (values.Count < 3)
				return;

			var data = new GyroscopeData(values[0], values[1], values[2]);
			Callback?.Invoke(data);
		}
	}
}
