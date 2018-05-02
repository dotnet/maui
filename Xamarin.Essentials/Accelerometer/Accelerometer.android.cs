using Android.Hardware;
using Android.Runtime;

namespace Xamarin.Essentials
{
    public static partial class Accelerometer
    {
        internal static bool IsSupported =>
            Platform.SensorManager?.GetDefaultSensor(SensorType.Accelerometer) != null;

        static AccelerometerListener listener;
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

            listener = new AccelerometerListener();
            accelerometer = Platform.SensorManager.GetDefaultSensor(SensorType.Accelerometer);
            Platform.SensorManager.RegisterListener(listener, accelerometer, delay);
        }

        internal static void PlatformStop()
        {
            if (listener == null || accelerometer == null)
                return;

            Platform.SensorManager.UnregisterListener(listener, accelerometer);
            listener.Dispose();
            listener = null;
        }
    }

    class AccelerometerListener : Java.Lang.Object, ISensorEventListener
    {
        // acceleration due to gravity
        const double gravity = 9.81;

        internal AccelerometerListener()
        {
        }

        void ISensorEventListener.OnAccuracyChanged(Sensor sensor, [GeneratedEnum] SensorStatus accuracy)
        {
        }

        void ISensorEventListener.OnSensorChanged(SensorEvent e)
        {
            var data = new AccelerometerData(e.Values[0] / gravity, e.Values[1] / gravity, e.Values[2] / gravity);
            Accelerometer.OnChanged(data);
        }
    }
}
