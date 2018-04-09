using System;
using CoreMotion;
using Foundation;

namespace Xamarin.Essentials
{
    public static partial class Magnetometer
    { // Timing intervales to match android sensor speeds in seconds
        // https://stackoverflow.com/questions/10044158/android-sensors
        internal const double FastestInterval = .02;
        internal const double GameInterval = .04;
        internal const double UiInterval = .08;
        internal const double NormalInterval = .225;

        internal static bool IsSupported =>
            Platform.MotionManager?.MagnetometerAvailable ?? false;

        internal static void PlatformStart(SensorSpeed sensorSpeed)
        {
            var manager = Platform.MotionManager;
            switch (sensorSpeed)
            {
                case SensorSpeed.Fastest:
                    manager.MagnetometerUpdateInterval = FastestInterval;
                    break;
                case SensorSpeed.Game:
                    manager.MagnetometerUpdateInterval = GameInterval;
                    break;
                case SensorSpeed.Normal:
                    manager.MagnetometerUpdateInterval = NormalInterval;
                    break;
                case SensorSpeed.Ui:
                    manager.MagnetometerUpdateInterval = UiInterval;
                    break;
            }

            manager.StartMagnetometerUpdates(Platform.GetCurrentQueue(), DataUpdated);
        }

        static void DataUpdated(CMMagnetometerData data, NSError error)
        {
            if (data == null)
                return;

            var field = data.MagneticField;
            var magnetometerData = new MagnetometerData(field.X, field.Y, field.Z);
            OnChanged(magnetometerData);
        }

        internal static void PlatformStop() =>
            Platform.MotionManager?.StopMagnetometerUpdates();
    }
}
