using System;
using CoreMotion;
using Foundation;

namespace Xamarin.Essentials
{
    public static partial class Gyroscope
    { // Timing intervales to match android sensor speeds in seconds
        // https://stackoverflow.com/questions/10044158/android-sensors
        internal const double FastestInterval = .02;
        internal const double GameInterval = .04;
        internal const double UiInterval = .08;
        internal const double NormalInterval = .225;

        internal static bool IsSupported =>
            Platform.MotionManager?.GyroAvailable ?? false;

        internal static void PlatformStart(SensorSpeed sensorSpeed)
        {
            var manager = Platform.MotionManager;
            switch (sensorSpeed)
            {
                case SensorSpeed.Fastest:
                    manager.GyroUpdateInterval = FastestInterval;
                    break;
                case SensorSpeed.Game:
                    manager.GyroUpdateInterval = GameInterval;
                    break;
                case SensorSpeed.Normal:
                    manager.GyroUpdateInterval = NormalInterval;
                    break;
                case SensorSpeed.Ui:
                    manager.GyroUpdateInterval = UiInterval;
                    break;
            }

            manager.StartGyroUpdates(NSOperationQueue.CurrentQueue, DataUpdated);
        }

        static void DataUpdated(CMGyroData data, NSError error)
        {
            if (data == null)
                return;

            var field = data.RotationRate;
            var gyroData = new GyroscopeData(field.x, field.y, field.z);
            OnChanged(gyroData);
        }

        internal static void PlatformStop() =>
            Platform.MotionManager?.StopGyroUpdates();
    }
}
