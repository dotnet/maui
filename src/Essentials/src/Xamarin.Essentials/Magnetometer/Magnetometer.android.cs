using Android.Hardware;
using Android.Runtime;

namespace Xamarin.Essentials
{
	public static partial class Magnetometer
	{
		internal static bool IsSupported =>
			   Platform.SensorManager?.GetDefaultSensor(SensorType.MagneticField) != null;

		static MagnetometerListener listener;
		static Sensor magnetometer;

		internal static void PlatformStart(SensorSpeed sensorSpeed)
		{
			var delay = sensorSpeed.ToPlatform();

			listener = new MagnetometerListener();
			magnetometer = Platform.SensorManager.GetDefaultSensor(SensorType.MagneticField);
			Platform.SensorManager.RegisterListener(listener, magnetometer, delay);
		}

		internal static void PlatformStop()
		{
			if (listener == null || magnetometer == null)
				return;

			Platform.SensorManager.UnregisterListener(listener, magnetometer);
			listener.Dispose();
			listener = null;
		}
	}

	class MagnetometerListener : Java.Lang.Object, ISensorEventListener
	{
		internal MagnetometerListener()
		{
		}

		void ISensorEventListener.OnAccuracyChanged(Sensor sensor, [GeneratedEnum] SensorStatus accuracy)
		{
		}

		void ISensorEventListener.OnSensorChanged(SensorEvent e)
		{
			if ((e?.Values?.Count ?? 0) < 3)
				return;

			var data = new MagnetometerData(e.Values[0], e.Values[1], e.Values[2]);
			Magnetometer.OnChanged(data);
		}
	}
}
