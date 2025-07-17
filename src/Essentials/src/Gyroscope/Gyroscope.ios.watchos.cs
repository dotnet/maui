#nullable enable
using CoreMotion;
using Foundation;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Devices.Sensors
{
	partial class GyroscopeImplementation : IGyroscope
	{
		static CMMotionManager? motionManager;

		static CMMotionManager MotionManager =>
			motionManager ??= new CMMotionManager();

		bool PlatformIsSupported =>
			MotionManager.GyroAvailable;

		void PlatformStart(SensorSpeed sensorSpeed)
		{
			MotionManager.GyroUpdateInterval = sensorSpeed.ToPlatform();
			MotionManager.StartGyroUpdates(NSOperationQueue.CurrentQueue ?? new NSOperationQueue(), DataUpdated);
		}

		void DataUpdated(CMGyroData? data, NSError? error)
		{
			if (data == null)
				return;

#pragma warning disable CA1416 // https://github.com/xamarin/xamarin-macios/issues/14619
			var field = data.RotationRate;
#pragma warning restore CA1416
			var gyroData = new GyroscopeData(field.x, field.y, field.z);
			RaiseReadingChanged(gyroData);
		}

		void PlatformStop() =>
			MotionManager.StopGyroUpdates();
	}
}
