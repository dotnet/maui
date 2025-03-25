#nullable enable
using System;
using Android.App;
using Android.Content;
using Android.Hardware;

namespace Microsoft.Maui.Devices.Sensors
{
	partial class BarometerImplementation : IBarometer
	{
		static Sensor? _sensor;

		static SensorManager? SensorManager => Application.Context.GetSystemService(Context.SensorService) as SensorManager;

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
		/// <summary>
		/// Initializes a new instance of the <see cref="BarometerListener"/> class.
		/// </summary>
		/// <param name="changeHandler">The handler that is invoked when a change in the barometer reading is detected.</param>
		public BarometerListener(Action<BarometerData> changeHandler)
		{
			ChangeHandler = changeHandler;
		}

		/// <summary>
		/// A reference to the action that invoked when a change in the barometer reading has been detected.
		/// </summary>
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
