using CoreMotion;
using Foundation;

namespace Xamarin.Essentials
{
    public static partial class OrientationSensor
    {
        internal static bool IsSupported =>
            Platform.MotionManager?.DeviceMotionAvailable ?? false;

        internal static void PlatformStart(SensorSpeed sensorSpeed)
        {
            var manager = Platform.MotionManager;
            manager.DeviceMotionUpdateInterval = sensorSpeed.ToPlatform();
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
