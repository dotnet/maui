using System;
using System.Linq;
using Android.Content;
using Android.Hardware;
using Android.Runtime;

namespace Microsoft.Maui.Controls.DualScreen
{
	public partial class HingeSensor
	{
		const string HINGE_SENSOR_TYPE = "microsoft.sensor.hinge_angle";

		SensorManager sensorManager;
		Sensor hingeSensor;
		HingeSensorEventListener sensorListener;

		public event EventHandler<HingeSensorChangedEventArgs> OnSensorChanged;

		public HingeSensor(Context context)
		{
			sensorManager = SensorManager.FromContext(context);

			var sensors = sensorManager.GetSensorList(SensorType.All);

			hingeSensor = sensors.FirstOrDefault(s => s.StringType.Equals(HINGE_SENSOR_TYPE, StringComparison.OrdinalIgnoreCase));
		}

		public bool HasHinge
			=> hingeSensor != null;

		public void StartListening()
		{
			if (sensorManager != null && hingeSensor != null)
			{
				if (sensorListener == null)
				{
					sensorListener = new HingeSensorEventListener
					{
						SensorChangedHandler = se =>
						{
							if (se.Sensor == hingeSensor)
								OnSensorChanged?.Invoke(hingeSensor, new HingeSensorChangedEventArgs(se));
						}
					};
				}

				sensorManager.RegisterListener(sensorListener, hingeSensor, SensorDelay.Normal);
			}
		}

		public void StopListening()
		{
			if (sensorManager != null && hingeSensor != null)
				sensorManager.UnregisterListener(sensorListener, hingeSensor);
		}

		class HingeSensorEventListener : Java.Lang.Object, ISensorEventListener
		{
			public Action<SensorEvent> SensorChangedHandler { get; set; }
			public Action<Sensor, SensorStatus> AccuracyChangedHandler { get; set; }

			public void OnAccuracyChanged(Sensor sensor, [GeneratedEnum] SensorStatus accuracy)
				=> AccuracyChangedHandler?.Invoke(sensor, accuracy);

			public void OnSensorChanged(SensorEvent e)
				=> SensorChangedHandler?.Invoke(e);
		}

		public class HingeSensorChangedEventArgs : EventArgs
		{
			public HingeSensorChangedEventArgs(SensorEvent sensorEvent)
			{
				SensorEvent = sensorEvent;
			}

			public SensorEvent SensorEvent { get; set; }

			public int HingeAngle
				=> (SensorEvent?.Values?.Count ?? 0) > 0 ? (int)SensorEvent.Values[0] : -1;
		}
	}
}