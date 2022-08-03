#nullable enable
using System;
using Android.App;
using Android.Content;
using Android.Hardware;

namespace Microsoft.Maui.Devices.Sensors
{
	partial class BarometerImplementation : IBarometer
	{
		static SensorManager? _sensorManager;
		static Sensor? _sensor;

		static SensorManager? SensorManager =>
			_sensorManager ??= Application.Context.GetSystemService(Context.SensorService) as SensorManager;

		static Sensor? Sensor =>
			_sensor ??= SensorManager?.GetDefaultSensor(SensorType.Pressure);

		public bool IsSupported => Sensor is not null;

		BarometerListener? _listener;

		void PlatformStart(SensorSpeed sensorSpeed)
		{
			_listener = new BarometerListener(RaiseReadingChanged);
			SensorManager!.RegisterListener(_listener, Sensor, sensorSpeed.ToPlatform());
		}

		void PlatformStop()
		{
			SensorManager!.UnregisterListener(_listener, Sensor);
			_listener!.Dispose();
			_listener = null;
		}
	}

	class BarometerListener : Java.Lang.Object, ISensorEventListener, IDisposable
	{
		public BarometerListener(Action<BarometerData> changeHandler)
		{
			ChangeHandler = changeHandler;
		}

		public readonly Action<BarometerData> ChangeHandler;

		void ISensorEventListener.OnAccuracyChanged(Sensor? sensor, SensorStatus accuracy)
		{
		}

		void ISensorEventListener.OnSensorChanged(SensorEvent? e)
		{
			var values = e?.Values ?? Array.Empty<float>();
			if (values.Count < 1)
				return;

			ChangeHandler?.Invoke(new BarometerData(values[0]));
		}
	}
}
