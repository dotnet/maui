using System;
using Android.Hardware;
using Android.Runtime;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Devices.Sensors
{
	public partial class BarometerImplementation : IBarometer
	{
		Sensor DefaultBarometer
			=> Platform.SensorManager?.GetDefaultSensor(SensorType.Pressure);
		
		Sensor barometer;
		BarometerListener listener;

		bool PlatformIsSupported
			=> DefaultBarometer != null;

		void PlatformStart(SensorSpeed sensorSpeed)
		{
			listener = new BarometerListener(RaiseReadingChanged);					
			barometer = DefaultBarometer;
			Platform.SensorManager.RegisterListener(listener, barometer, sensorSpeed.ToPlatform());
		}

		void PlatformStop()
		{
			if (listener == null)
				return;
			
			Platform.SensorManager.UnregisterListener(listener, barometer);
			listener.Dispose();
			listener = null;
			barometer = null;
		}
	}

	class BarometerListener : Java.Lang.Object, ISensorEventListener, IDisposable
	{
		public BarometerListener(Action<BarometerData> changeHandler)
		{
			ChangeHandler = changeHandler;
		}

		public readonly Action<BarometerData> ChangeHandler;

		void ISensorEventListener.OnAccuracyChanged(Sensor sensor, [GeneratedEnum] SensorStatus accuracy)
		{
		}

		void ISensorEventListener.OnSensorChanged(SensorEvent e)
		{
			if ((e?.Values?.Count ?? 0) <= 0)
				return;

			ChangeHandler?.Invoke(new BarometerData(e.Values[0]));
		}
	}
}
