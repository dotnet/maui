using CoreMotion;
using Foundation;

namespace Xamarin.Essentials
{
    public static partial class OrientationSensor
    {
        // Timing intervales to match android sensor speeds in seconds
        // https://stackoverflow.com/questions/10044158/android-sensors
        internal const double FastestInterval = .02;
        internal const double GameInterval = .04;
        internal const double UiInterval = .08;
        internal const double NormalInterval = .225;

        internal static bool IsSupported =>
            Platform.MotionManager?.DeviceMotionAvailable ?? false;

        internal static void PlatformStart(SensorSpeed sensorSpeed)
        {
            var manager = Platform.MotionManager;
            switch (sensorSpeed)
            {
                case SensorSpeed.Fastest:
                    manager.DeviceMotionUpdateInterval = FastestInterval;
                    break;
                case SensorSpeed.Game:
                    manager.DeviceMotionUpdateInterval = GameInterval;
                    break;
                case SensorSpeed.Normal:
                    manager.DeviceMotionUpdateInterval = NormalInterval;
                    break;
                case SensorSpeed.Ui:
                    manager.DeviceMotionUpdateInterval = UiInterval;
                    break;
            }

            manager.StartDeviceMotionUpdates(Platform.GetCurrentQueue(), DataUpdated);
        }

        static void DataUpdated(CMDeviceMotion data, NSError error)
        {
            if (data == null)
                return;

            var field = data.Attitude.Quaternion;
            var rotationData = new OrientationSensorData(field.x, field.y, field.z, field.w);
            OnChanged(rotationData);
        }

        internal static void PlatformStop() =>
            Platform.MotionManager?.StopDeviceMotionUpdates();
    }
}
