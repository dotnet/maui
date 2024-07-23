#nullable enable
using System;
using Android.App;
using Android.Content;
using Android.Hardware;
using Android.Runtime;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Devices.Sensors
{
	partial class MagnetometerImplementation : IMagnetometer
	{
		static Sensor? magnetometer;

		static SensorManager? SensorManager =>
			Application.Context.GetSystemService(Context.SensorService) as SensorManager;

		static Sensor? Sensor =>
			magnetometer ??= SensorManager?.GetDefaultSensor(SensorType.MagneticField);

		bool PlatformIsSupported =>
			Sensor is not null;

		MagnetometerListener? listener;

		void PlatformStart(SensorSpeed sensorSpeed)
		{
			var delay = sensorSpeed.ToPlatform();

			listener = new MagnetometerListener(RaiseReadingChanged);
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

	class MagnetometerListener : Java.Lang.Object, ISensorEventListener
	{
		internal MagnetometerListener(Action<MagnetometerData> callback)
		{
			DataCallback = callback;
		}

		readonly Action<MagnetometerData> DataCallback;

		void ISensorEventListener.OnAccuracyChanged(Sensor? sensor, SensorStatus accuracy)
		{
		}

		void ISensorEventListener.OnSensorChanged(SensorEvent? e)
		{
			var values = e?.Values ?? Array.Empty<float>();
			if (values.Count < 3)
				return;

			var data = new MagnetometerData(values[0], values[1], values[2]);
			DataCallback?.Invoke(data);
		}
	}
}
