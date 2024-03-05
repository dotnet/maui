using System;
using System.Linq;
using Android.Content;
using Android.Hardware;
using Android.Runtime;

namespace Microsoft.Maui.Foldable
{
	/// <summary>
	/// Sensor associated with the hinge angle between two halves of a folding device
	/// like the Microsoft Surface Duo
	/// </summary>
	/// <remarks>
	/// Previously used a Microsoft-specific hinge sensor identifier:
	/// <code>
	/// // This string can ONLY detect the hinge on Microsoft Surface Duo devices
	/// //and must be used with a comparison to the `sensor.StringType`
	/// const string HINGE_SENSOR_TYPE = "microsoft.sensor.hinge_angle";
	/// </code>
	/// Removed LINQ too:
	/// <code>
	/// // Replaced "microsoft.sensor.hinge_angle"-specific comparison
	///  hingeSensor = sensors.FirstOrDefault(s => s.StringType.Equals(HINGE_SENSOR_TYPE, StringComparison.OrdinalIgnoreCase));
	/// </code>
	/// </remarks>
	public partial class HingeSensor
	{
		// This string will detect hinge sensors on other foldable devices
		// and should be used with a comparison to the `sensor.Name`
		const string HINGE_SENSOR_NAME = "Hinge";

		SensorManager sensorManager;
		Sensor hingeSensor;
		HingeSensorEventListener sensorListener;

		public event EventHandler<HingeSensorChangedEventArgs> OnSensorChanged;

		public HingeSensor(Context context)
		{
			sensorManager = SensorManager.FromContext(context);

			var sensors = sensorManager.GetSensorList(SensorType.All);

			foreach (var sensor in sensors)
			{ // Use generic "hinge" sensor name, to match on a variety of folding device types
				if (sensor.Name.Contains(HINGE_SENSOR_NAME, StringComparison.InvariantCultureIgnoreCase))
				{
					hingeSensor = sensor;
					break;
				}
			}
		}

		public bool HasHinge
			=> hingeSensor != null;

		public void StartListening()
		{
			if (sensorManager != null && hingeSensor != null)
			{
				sensorListener ??= new HingeSensorEventListener
				{
					SensorChangedHandler = se =>
					{
						if (se.Sensor == hingeSensor)
						{
							OnSensorChanged?.Invoke(hingeSensor, new HingeSensorChangedEventArgs(se));
						}
					}
				};

				sensorManager.RegisterListener(sensorListener, hingeSensor, SensorDelay.Normal);
			}
		}

		public void StopListening()
		{
			if (sensorManager != null && hingeSensor != null)
			{
				sensorManager.UnregisterListener(sensorListener, hingeSensor);
			}
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