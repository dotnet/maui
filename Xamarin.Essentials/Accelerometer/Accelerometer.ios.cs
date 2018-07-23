using CoreMotion;
using Foundation;

namespace Xamarin.Essentials
{
    public static partial class Accelerometer
    {
        // Timing intervales to match android sensor speeds in seconds
        // https://stackoverflow.com/questions/10044158/android-sensors
        internal const double FastestInterval = .02;
        internal const double GameInterval = .04;
        internal const double UIInterval = .08;
        internal const double NormalInterval = .225;

        internal static bool IsSupported =>
            Platform.MotionManager?.AccelerometerAvailable ?? false;

        internal static void PlatformStart(SensorSpeed sensorSpeed)
        {
            var manager = Platform.MotionManager;
            switch (sensorSpeed)
            {
                case SensorSpeed.Fastest:
                    manager.AccelerometerUpdateInterval = FastestInterval;
                    break;
                case SensorSpeed.Game:
                    manager.AccelerometerUpdateInterval = GameInterval;
                    break;
                case SensorSpeed.Normal:
                    manager.AccelerometerUpdateInterval = NormalInterval;
                    break;
                case SensorSpeed.UI:
                    manager.AccelerometerUpdateInterval = UIInterval;
                    break;
            }

            manager.StartAccelerometerUpdates(Platform.GetCurrentQueue(), DataUpdated);
        }

        static void DataUpdated(CMAccelerometerData data, NSError error)
        {
            if (data == null)
                return;

            var field = data.Acceleration;
            var accelData = new AccelerometerData(field.X * -1, field.Y * -1, field.Z * -1);
            OnChanged(accelData);
        }

        internal static void PlatformStop() =>
            Platform.MotionManager?.StopAccelerometerUpdates();
    }
}
