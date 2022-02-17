using System;
using Android.Hardware;
using Android.Runtime;

namespace Microsoft.Maui.Essentials.Implementations
{
	public partial class BarometerImplementation : IBarometer
	{
		static Sensor DefaultBarometer 
			=> Platform.SensorManager?.GetDefaultSensor(SensorType.Pressure);
		
		static Sensor barometer;
		static BarometerListener listener;

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
