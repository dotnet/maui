using System;
using System.Collections.Generic;
using Android.Hardware;
using Android.Runtime;

namespace Xamarin.Essentials
{
    public static partial class Compass
    {
        internal static bool IsSupported =>
            Platform.SensorManager?.GetDefaultSensor(SensorType.Accelerometer) != null &&
            Platform.SensorManager?.GetDefaultSensor(SensorType.MagneticField) != null;

        static SensorListener listener;
        static Sensor magnetometer;
        static Sensor accelerometer;

        internal static void PlatformStart(SensorSpeed sensorSpeed)
        {
            var delay = SensorDelay.Normal;
            switch (sensorSpeed)
            {
                case SensorSpeed.Normal:
                    delay = SensorDelay.Normal;
                    break;
                case SensorSpeed.Fastest:
                    delay = SensorDelay.Fastest;
                    break;
                case SensorSpeed.Game:
                    delay = SensorDelay.Game;
                    break;
                case SensorSpeed.Ui:
                    delay = SensorDelay.Ui;
                    break;
            }

            accelerometer = Platform.SensorManager.GetDefaultSensor(SensorType.Accelerometer);
            magnetometer = Platform.SensorManager.GetDefaultSensor(SensorType.MagneticField);
            listener = new SensorListener(accelerometer.Name, magnetometer.Name, delay);
            Platform.SensorManager.RegisterListener(listener, accelerometer, delay);
            Platform.SensorManager.RegisterListener(listener, magnetometer, delay);
        }

        internal static void PlatformStop()
        {
            if (listener == null)
                return;

            Platform.SensorManager.UnregisterListener(listener, accelerometer);
            Platform.SensorManager.UnregisterListener(listener, magnetometer);
            listener.Dispose();
            listener = null;
        }
    }

    internal class SensorListener : Java.Lang.Object, ISensorEventListener, IDisposable
    {
        float[] lastAccelerometer = new float[3];
        float[] lastMagnetometer = new float[3];
        bool lastAccelerometerSet;
        bool lastMagnetometerSet;
        float[] r = new float[9];
        float[] orientation = new float[3];

        string magnetometer;
        string accelerometer;

        internal SensorListener(string accelerometer, string magnetometer, SensorDelay delay)
        {
            this.magnetometer = magnetometer;
            this.accelerometer = accelerometer;
        }

        public void OnAccuracyChanged(Sensor sensor, [GeneratedEnum] SensorStatus accuracy)
        {
        }

        public void OnSensorChanged(SensorEvent e)
        {
            if (e.Sensor.Name == accelerometer && !lastAccelerometerSet)
            {
                CopyValues(e.Values, lastAccelerometer);
                lastAccelerometerSet = true;
            }
            else if (e.Sensor.Name == magnetometer && !lastMagnetometerSet)
            {
                CopyValues(e.Values, lastMagnetometer);
                lastMagnetometerSet = true;
            }

            if (lastAccelerometerSet && lastMagnetometerSet)
            {
                SensorManager.GetRotationMatrix(r, null, lastAccelerometer, lastMagnetometer);
                SensorManager.GetOrientation(r, orientation);
                var azimuthInRadians = orientation[0];
                var azimuthInDegress = (Java.Lang.Math.ToDegrees(azimuthInRadians) + 360.0) % 360.0;

                var data = new CompassData(azimuthInDegress);
                Compass.OnChanged(data);
                lastMagnetometerSet = false;
                lastAccelerometerSet = false;
            }
        }

        void CopyValues(IList<float> source, float[] destination)
        {
            for (var i = 0; i < source.Count; ++i)
            {
                destination[i] = source[i];
            }
        }
    }
}
