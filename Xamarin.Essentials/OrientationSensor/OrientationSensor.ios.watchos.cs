using System;
using System.Numerics;
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

            // use a fixed reference frame where X points north and Z points vertically into the sky
            manager.StartDeviceMotionUpdates(CMAttitudeReferenceFrame.XTrueNorthZVertical, Platform.GetCurrentQueue(), DataUpdated);
        }

        static void DataUpdated(CMDeviceMotion data, NSError error)
        {
            if (data == null)
                return;

            var field = data.Attitude.Quaternion;

            // the quaternion returned by the MotionManager refers to a frame where the X axis points north
            var q = new Quaternion((float)field.x, (float)field.y, (float)field.z, (float)field.w);

            // we rotate it by 90 degrees around the Z axis, so that the Y axis points north and the X axis points east
            var qz90 = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, (float)(Math.PI / 2.0));
            q = Quaternion.Multiply(q, qz90);
            var rotationData = new OrientationSensorData(q.X, q.Y, q.Z, q.W);
            OnChanged(rotationData);
        }

        internal static void PlatformStop() =>
            Platform.MotionManager?.StopDeviceMotionUpdates();
    }
}
