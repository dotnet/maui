using System;
using CoreMotion;
using Foundation;

namespace Xamarin.Essentials
{
    public static partial class Accelerometer
    { // Timing intervales to match android sensor speeds in seconds
        // https://stackoverflow.com/questions/10044158/android-sensors
        internal const double FastestInterval = .02;
        internal const double GameInterval = .04;
        internal const double UiInterval = .08;
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
                case SensorSpeed.Ui:
                    manager.AccelerometerUpdateInterval = UiInterval;
                    break;
            }

            manager.StartAccelerometerUpdates(Platform.GetCurrentQueue(), DataUpdated);
        }

        static void DataUpdated(CMAccelerometerData data, NSError error)
        {
            if (data == null)
                return;

            var field = data.Acceleration;
            var accelData = new AccelerometerData(field.X, field.Y, field.Z);
            OnChanged(accelData);
        }

        internal static void PlatformStop() =>
            Platform.MotionManager?.StopAccelerometerUpdates();
    }
}
