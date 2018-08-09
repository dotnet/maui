using CoreMotion;
using Foundation;

namespace Xamarin.Essentials
{
    public static partial class Magnetometer
    {
        internal static bool IsSupported =>
            Platform.MotionManager?.MagnetometerAvailable ?? false;

        internal static void PlatformStart(SensorSpeed sensorSpeed)
        {
            var manager = Platform.MotionManager;
            manager.MagnetometerUpdateInterval = sensorSpeed.ToPlatform();
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
