using System;
using Android.Hardware;
using Android.Runtime;

namespace Microsoft.Maui.Essentials.Implementations
{
	public partial class GyroscopeImplementation : IGyroscope
	{
		bool PlatformIsSupported =>
			   Platform.SensorManager?.GetDefaultSensor(SensorType.Gyroscope) != null;

		GyroscopeListener listener;
		Sensor gyroscope;

		void PlatformStart(SensorSpeed sensorSpeed)
		{
			var delay = sensorSpeed.ToPlatform();

			listener = new GyroscopeListener(RaiseReadingChanged);
			gyroscope = Platform.SensorManager.GetDefaultSensor(SensorType.Gyroscope);
			Platform.SensorManager.RegisterListener(listener, gyroscope, delay);
		}

		void PlatformStop()
		{
			if (listener == null || gyroscope == null)
				return;

			Platform.SensorManager.UnregisterListener(listener, gyroscope);
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

		void ISensorEventListener.OnAccuracyChanged(Sensor sensor, [GeneratedEnum] SensorStatus accuracy)
		{
		}

		void ISensorEventListener.OnSensorChanged(SensorEvent e)
		{
			if ((e?.Values?.Count ?? 0) < 3)
				return;

			var data = new GyroscopeData(e.Values[0], e.Values[1], e.Values[2]);
			Callback?.Invoke(data);
		}
	}
}
