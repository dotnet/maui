using CoreMotion;
using Foundation;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Devices.Sensors
{
	public partial class AccelerometerImplementation
	{
		public bool IsSupported =>
			Platform.MotionManager?.AccelerometerAvailable ?? false;

		void PlatformStart(SensorSpeed sensorSpeed)
		{
			var manager = Platform.MotionManager;
			manager.AccelerometerUpdateInterval = sensorSpeed.ToPlatform();
			manager.StartAccelerometerUpdates(Platform.GetCurrentQueue(), DataUpdated);
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
			Platform.MotionManager?.StopAccelerometerUpdates();
	}
}
