using System;
using Android.App;
using Android.Content;
using Android.Hardware;

namespace Microsoft.Maui.Devices.Sensors
{
	partial class CompassImplementation : ICompass
	{
		static Sensor _accelerometer;
		static Sensor _magnetic;

		static SensorManager SensorManager =>
			Application.Context.GetSystemService(Context.SensorService) as SensorManager;

		static Sensor Accelerometer =>
			_accelerometer ??= SensorManager?.GetDefaultSensor(SensorType.Accelerometer);

		static Sensor MagneticField =>
			_magnetic ??= SensorManager?.GetDefaultSensor(SensorType.MagneticField);

		bool PlatformIsSupported => Accelerometer is not null && MagneticField is not null;

		SensorListener listener;

		void PlatformStart(SensorSpeed sensorSpeed, bool applyLowPassFilter)
		{
			var delay = sensorSpeed.ToPlatform();
			listener = new SensorListener(Accelerometer.Name, MagneticField.Name, delay, applyLowPassFilter, RaiseReadingChanged);
			SensorManager.RegisterListener(listener, Accelerometer, delay);
			SensorManager.RegisterListener(listener, MagneticField, delay);
		}

		void PlatformStop()
		{
			if (listener == null)
				return;

			SensorManager.UnregisterListener(listener, Accelerometer);
			SensorManager.UnregisterListener(listener, MagneticField);
			listener.Dispose();
			listener = null;
		}
	}

	class SensorListener : Java.Lang.Object, ISensorEventListener, IDisposable
	{
		LowPassFilter filter = new LowPassFilter();
		float[] lastAccelerometer = new float[3];
		float[] lastMagnetometer = new float[3];
		bool lastAccelerometerSet;
		bool lastMagnetometerSet;
		float[] r = new float[9];
		float[] orientation = new float[3];

		string magnetometer;
		string accelerometer;
		bool applyLowPassFilter;

		Action<CompassData> callback;

		internal SensorListener(string accelerometer, string magnetometer, SensorDelay delay, bool applyLowPassFilter, Action<CompassData> callback)
		{
			this.magnetometer = magnetometer;
			this.accelerometer = accelerometer;
			this.applyLowPassFilter = applyLowPassFilter;
			this.callback = callback;
		}

		void ISensorEventListener.OnAccuracyChanged(Sensor sensor, SensorStatus accuracy)
		{
		}

		void ISensorEventListener.OnSensorChanged(SensorEvent e)
		{
			if (e.Sensor.Name == accelerometer && !lastAccelerometerSet)
			{
				e.Values.CopyTo(lastAccelerometer, 0);
				lastAccelerometerSet = true;
			}
			else if (e.Sensor.Name == magnetometer && !lastMagnetometerSet)
			{
				e.Values.CopyTo(lastMagnetometer, 0);
				lastMagnetometerSet = true;
			}

			if (lastAccelerometerSet && lastMagnetometerSet)
			{
				SensorManager.GetRotationMatrix(r, null, lastAccelerometer, lastMagnetometer);
				SensorManager.GetOrientation(r, orientation);

				if (orientation.Length <= 0)
					return;

				var azimuthInRadians = orientation[0];
				if (applyLowPassFilter)
				{
					filter.Add(azimuthInRadians);
					azimuthInRadians = filter.Average();
				}
				var azimuthInDegress = (Java.Lang.Math.ToDegrees(azimuthInRadians) + 360.0) % 360.0;

				var data = new CompassData(azimuthInDegress);
				callback?.Invoke(data);
				lastMagnetometerSet = false;
				lastAccelerometerSet = false;
			}
		}
	}
}
