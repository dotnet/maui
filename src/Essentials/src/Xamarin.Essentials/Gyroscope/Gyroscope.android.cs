using Android.Hardware;
using Android.Runtime;

namespace Xamarin.Essentials
{
	public static partial class Gyroscope
	{
		internal static bool IsSupported =>
			   Platform.SensorManager?.GetDefaultSensor(SensorType.Gyroscope) != null;

		static GyroscopeListener listener;
		static Sensor gyroscope;

		internal static void PlatformStart(SensorSpeed sensorSpeed)
		{
			var delay = sensorSpeed.ToPlatform();

			listener = new GyroscopeListener();
			gyroscope = Platform.SensorManager.GetDefaultSensor(SensorType.Gyroscope);
			Platform.SensorManager.RegisterListener(listener, gyroscope, delay);
		}

		internal static void PlatformStop()
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
		internal GyroscopeListener()
		{
		}

		void ISensorEventListener.OnAccuracyChanged(Sensor sensor, [GeneratedEnum] SensorStatus accuracy)
		{
		}

		void ISensorEventListener.OnSensorChanged(SensorEvent e)
		{
			if ((e?.Values?.Count ?? 0) < 3)
				return;

			var data = new GyroscopeData(e.Values[0], e.Values[1], e.Values[2]);
			Gyroscope.OnChanged(data);
		}
	}
}
