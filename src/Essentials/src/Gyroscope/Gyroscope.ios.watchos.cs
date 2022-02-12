using CoreMotion;
using Foundation;

namespace Microsoft.Maui.Essentials.Implementations
{
	public partial class GyroscopeImplementation : IGyroscope
	{
		public bool IsSupported =>
			Platform.MotionManager?.GyroAvailable ?? false;

		public void Start(SensorSpeed sensorSpeed)
		{
			var manager = Platform.MotionManager;
			manager.GyroUpdateInterval = sensorSpeed.ToPlatform();
			manager.StartGyroUpdates(Platform.GetCurrentQueue(), DataUpdated);
		}

		static void DataUpdated(CMGyroData data, NSError error)
		{
			if (data == null)
				return;

			var field = data.RotationRate;
			var gyroData = new GyroscopeData(field.x, field.y, field.z);
			Gyroscope.OnChanged(gyroData);
		}

		public void Stop() =>
			Platform.MotionManager?.StopGyroUpdates();
	}
}
