using System;
using System.Numerics;
using CoreMotion;
using Foundation;

namespace Microsoft.Maui.Essentials
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

			// the quaternion returned by the MotionManager refers to a frame where the X axis points north ("iOS frame")
			var q = new Quaternion((float)field.x, (float)field.y, (float)field.z, (float)field.w);

			// in Xamarin, the reference frame is defined such that Y points north and Z is vertical,
			// so we first rotate by 90 degrees around the Z axis, in order to get from the Xamarin frame to the iOS frame
			var qz90 = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, (float)(Math.PI / 2.0));

			// on top of this rotation, we apply the actual quaternion obtained from the MotionManager,
			// so that the final quaternion will take us from the earth frame in Xamarin convention to the phone frame
			q = Quaternion.Multiply(qz90, q);
			var rotationData = new OrientationSensorData(q.X, q.Y, q.Z, q.W);
			OnChanged(rotationData);
		}

		internal static void PlatformStop() =>
			Platform.MotionManager?.StopDeviceMotionUpdates();
	}
}
