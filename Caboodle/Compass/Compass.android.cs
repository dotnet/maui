using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Android.Hardware;
using Android.Runtime;

namespace Microsoft.Caboodle
{
    public static partial class Compass
    {
        internal static bool IsSupported =>
            Platform.SensorManager?.GetDefaultSensor(SensorType.Accelerometer) != null &&
            Platform.SensorManager?.GetDefaultSensor(SensorType.MagneticField) != null;

        internal static void PlatformStart(SensorSpeed sensorSpeed, Action<CompassData> handler)
        {
            var delay = SensorDelay.Normal;
            var useSyncContext = false;
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
                    useSyncContext = true;
                    break;
                case SensorSpeed.Ui:
                    delay = SensorDelay.Ui;
                    useSyncContext = true;
                    break;
            }

            var sensorListener = new SensorListener(useSyncContext, handler, delay);

            MonitorCTS.Token.Register(CancelledToken, useSyncContext);

            void CancelledToken()
            {
                sensorListener.Dispose();
                sensorListener = null;
                DisposeToken();
            }
        }
    }

    internal class SensorListener : Java.Lang.Object, ISensorEventListener, IDisposable
    {
        Action<CompassData> handler;
        float[] lastAccelerometer = new float[3];
        float[] lastMagnetometer = new float[3];
        bool lastAccelerometerSet;
        bool lastMagnetometerSet;
        float[] r = new float[9];
        float[] orientation = new float[3];
        bool useSyncContext;
        Sensor accelerometer;
        Sensor magnetometer;

        internal SensorListener(bool useSyncContext, Action<CompassData> handler, SensorDelay delay)
        {
            this.handler = handler;
            this.useSyncContext = useSyncContext;

            accelerometer = Platform.SensorManager.GetDefaultSensor(SensorType.Accelerometer);
            magnetometer = Platform.SensorManager.GetDefaultSensor(SensorType.MagneticField);
            Platform.SensorManager.RegisterListener(this, accelerometer, delay);
            Platform.SensorManager.RegisterListener(this, magnetometer, delay);
        }

        public void OnAccuracyChanged(Sensor sensor, [GeneratedEnum] SensorStatus accuracy)
        {
        }

        public void OnSensorChanged(SensorEvent e)
        {
            if (e.Sensor == accelerometer && !lastAccelerometerSet)
            {
                CopyValues(e.Values, lastAccelerometer);
                lastAccelerometerSet = true;
            }
            else if (e.Sensor == magnetometer && !lastMagnetometerSet)
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
                if (useSyncContext)
                {
                    Platform.BeginInvokeOnMainThread(() => handler?.Invoke(data));
                }
                else
                {
                    handler?.Invoke(data);
                }
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

        bool disposed = false;

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposed)
            {
                if (disposing)
                {
                    Platform.SensorManager.UnregisterListener(this, accelerometer);
                    Platform.SensorManager.UnregisterListener(this, magnetometer);
                }

                disposed = true;
            }
        }
    }
}
