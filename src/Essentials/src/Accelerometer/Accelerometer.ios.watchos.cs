#nullable enable
using CoreMotion;
using Foundation;

namespace Microsoft.Maui.Devices.Sensors
{
	partial class AccelerometerImplementation
	{
		static CMMotionManager? motionManager;

		static CMMotionManager MotionManager =>
			motionManager ??= new CMMotionManager();

		public bool IsSupported =>
			MotionManager.AccelerometerAvailable;

		void PlatformStart(SensorSpeed sensorSpeed)
		{
			MotionManager.AccelerometerUpdateInterval = sensorSpeed.ToPlatform();
			MotionManager.StartAccelerometerUpdates(NSOperationQueue.CurrentQueue, DataUpdated);
		}

		void DataUpdated(CMAccelerometerData data, NSError error)
		{
			if (data == null)
				return;

			var field = data.Acceleration;
			var accelData = new AccelerometerData(field.X * -1, field.Y * -1, field.Z * -1);
			OnChanged(accelData);
		}

		void PlatformStop() =>
			MotionManager.StopAccelerometerUpdates();
	}
}
