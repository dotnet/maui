using System;
using Android.Hardware;
using Android.Runtime;

namespace Microsoft.Maui.Essentials.Implementations
{
	public class BarometerImplementation : IBarometer
	{
		public bool IsSupported
			=> DefaultBarometer != null;

		public event EventHandler<BarometerChangedEventArgs> ReadingChanged;

		static Sensor DefaultBarometer 
			=> Platform.SensorManager?.GetDefaultSensor(SensorType.Pressure);
		
		static Sensor barometer;
		static BarometerListener listener;

		bool UseSyncContext => SensorSpeed == SensorSpeed.Default || SensorSpeed == SensorSpeed.UI;

		public SensorSpeed SensorSpeed { get; private set; } = SensorSpeed.Default;

		public bool IsMonitoring { get; private set; }

		public void Start(SensorSpeed sensorSpeed)
		{
			if (!IsSupported)
				throw new FeatureNotSupportedException();

			if (IsMonitoring)
				throw new InvalidOperationException("Barometer has already been started.");

			IsMonitoring = true;
			SensorSpeed = sensorSpeed;

			try
			{
				listener = new BarometerListener(data =>
				{
					if (UseSyncContext)
						MainThread.BeginInvokeOnMainThread(() => ReadingChanged?.Invoke(this, new BarometerChangedEventArgs(data)));
					else
						ReadingChanged?.Invoke(this, new BarometerChangedEventArgs(data));
				});
					
				barometer = DefaultBarometer;
				Platform.SensorManager.RegisterListener(listener, barometer, sensorSpeed.ToPlatform());
			}
			catch
			{
				IsMonitoring = false;
				throw;
			}
		}

		public void Stop()
		{
			if (!IsSupported)
				throw new FeatureNotSupportedException();

			if (!IsMonitoring)
				return;

			IsMonitoring = false;

			if (listener == null)
				return;

			try
			{
				Platform.SensorManager.UnregisterListener(listener, barometer);
				listener.Dispose();
				listener = null;
			}
			catch
			{
				IsMonitoring = true;
				throw;
			}
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
