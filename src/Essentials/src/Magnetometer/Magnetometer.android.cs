using System;
using Android.Hardware;
using Android.Runtime;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Devices.Sensors
{
	public partial class MagnetometerImplementation : IMagnetometer
	{
		bool PlatformIsSupported =>
			   Platform.SensorManager?.GetDefaultSensor(SensorType.MagneticField) != null;

		MagnetometerListener listener;
		Sensor magnetometer;

		void PlatformStart(SensorSpeed sensorSpeed)
		{
			var delay = sensorSpeed.ToPlatform();

			listener = new MagnetometerListener(RaiseReadingChanged);
			magnetometer = Platform.SensorManager.GetDefaultSensor(SensorType.MagneticField);
			Platform.SensorManager.RegisterListener(listener, magnetometer, delay);
		}

		void PlatformStop()
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
		internal MagnetometerListener(Action<MagnetometerData> callback)
		{
			DataCallback = callback;
		}

		readonly Action<MagnetometerData> DataCallback;

		void ISensorEventListener.OnAccuracyChanged(Sensor sensor, [GeneratedEnum] SensorStatus accuracy)
		{
		}

		void ISensorEventListener.OnSensorChanged(SensorEvent e)
		{
			if ((e?.Values?.Count ?? 0) < 3)
				return;

			var data = new MagnetometerData(e.Values[0], e.Values[1], e.Values[2]);
			DataCallback?.Invoke(data);
		}
	}
}
