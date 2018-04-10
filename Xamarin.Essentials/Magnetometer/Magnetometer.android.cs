using Android.Hardware;
using Android.Runtime;

namespace Xamarin.Essentials
{
    public static partial class Magnetometer
    {
        internal static bool IsSupported =>
               Platform.SensorManager?.GetDefaultSensor(SensorType.MagneticField) != null;

        static MagnetometerListener listener;
        static Sensor magnetometer;

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

            listener = new MagnetometerListener();
            magnetometer = Platform.SensorManager.GetDefaultSensor(SensorType.MagneticField);
            Platform.SensorManager.RegisterListener(listener, magnetometer, delay);
        }

        internal static void PlatformStop()
        {
            if (listener == null || magnetometer == null)
            {
                return;
            }

            Platform.SensorManager.UnregisterListener(listener, magnetometer);
            listener.Dispose();
            listener = null;
        }
    }

    class MagnetometerListener : Java.Lang.Object, ISensorEventListener
    {
        public MagnetometerListener()
        {
        }

        public void OnAccuracyChanged(Sensor sensor, [GeneratedEnum] SensorStatus accuracy)
        {
        }

        public void OnSensorChanged(SensorEvent e)
        {
            var data = new MagnetometerData(e.Values[0], e.Values[1], e.Values[2]);
            Magnetometer.OnChanged(data);
        }
    }
}
