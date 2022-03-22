#nullable enable
using CoreMotion;
using Foundation;

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
			MotionManager.StartGyroUpdates(NSOperationQueue.CurrentQueue, DataUpdated);
		}

		void DataUpdated(CMGyroData data, NSError error)
		{
			if (data == null)
				return;

			var field = data.RotationRate;
			var gyroData = new GyroscopeData(field.x, field.y, field.z);
			RaiseReadingChanged(gyroData);
		}

		void PlatformStop() =>
			MotionManager.StopGyroUpdates();
	}
}
