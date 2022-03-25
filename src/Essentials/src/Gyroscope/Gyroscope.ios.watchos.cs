using CoreMotion;
using Foundation;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Devices.Sensors
{
	public partial class GyroscopeImplementation : IGyroscope
	{
		bool PlatformIsSupported =>
			Platform.MotionManager?.GyroAvailable ?? false;

		void PlatformStart(SensorSpeed sensorSpeed)
		{
			var manager = Platform.MotionManager;
			manager.GyroUpdateInterval = sensorSpeed.ToPlatform();
			manager.StartGyroUpdates(Platform.GetCurrentQueue(), DataUpdated);
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
			Platform.MotionManager?.StopGyroUpdates();
	}
}
