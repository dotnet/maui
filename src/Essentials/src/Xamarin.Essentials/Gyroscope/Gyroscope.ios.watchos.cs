using CoreMotion;
using Foundation;

namespace Xamarin.Essentials
{
	public static partial class Gyroscope
	{
		internal static bool IsSupported =>
			Platform.MotionManager?.GyroAvailable ?? false;

		internal static void PlatformStart(SensorSpeed sensorSpeed)
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
			OnChanged(gyroData);
		}

		internal static void PlatformStop() =>
			Platform.MotionManager?.StopGyroUpdates();
	}
}
