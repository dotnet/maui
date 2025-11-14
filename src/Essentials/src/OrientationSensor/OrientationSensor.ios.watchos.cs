#nullable enable
using System;
using System.Numerics;
using CoreMotion;
using Foundation;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Devices.Sensors
{
	partial class OrientationSensorImplementation : IOrientationSensor
	{
		static CMMotionManager? motionManager;

		static CMMotionManager MotionManager =>
			motionManager ??= new CMMotionManager();

		bool PlatformIsSupported =>
			MotionManager.GyroAvailable;

		void PlatformStart(SensorSpeed sensorSpeed)
		{
			MotionManager.DeviceMotionUpdateInterval = sensorSpeed.ToPlatform();

			// use a fixed reference frame where X points north and Z points vertically into the sky
			MotionManager.StartDeviceMotionUpdates(CMAttitudeReferenceFrame.XTrueNorthZVertical, NSOperationQueue.CurrentQueue ?? new NSOperationQueue(), DataUpdated);
		}

		void DataUpdated(CMDeviceMotion? data, NSError? error)
		{
			if (data == null)
				return;

#pragma warning disable CA1416 // https://github.com/xamarin/xamarin-macios/issues/14619
			var field = data.Attitude.Quaternion;
#pragma warning restore CA1416

			// the quaternion returned by the MotionManager refers to a frame where the X axis points north ("iOS frame")
			var q = new Quaternion((float)field.x, (float)field.y, (float)field.z, (float)field.w);

			// In .NET MAUI, the reference frame is defined such that Y points north and Z is vertical,
			// so we first rotate by 90 degrees around the Z axis, in order to get from the .NET MAUI frame to the iOS frame
			var qz90 = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, (float)(Math.PI / 2.0));

			// on top of this rotation, we apply the actual quaternion obtained from the MotionManager,
			// so that the final quaternion will take us from the earth frame in .NET MAUI convention to the phone frame
			q = Quaternion.Multiply(qz90, q);
			var rotationData = new OrientationSensorData(q.X, q.Y, q.Z, q.W);
			RaiseReadingChanged(rotationData);
		}

		void PlatformStop() =>
			MotionManager.StopDeviceMotionUpdates();
	}
}
