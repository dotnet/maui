using CoreMotion;
using Foundation;

namespace Xamarin.Essentials
{
	public static partial class Accelerometer
	{
		internal static bool IsSupported =>
			Platform.MotionManager?.AccelerometerAvailable ?? false;

		internal static void PlatformStart(SensorSpeed sensorSpeed)
		{
			var manager = Platform.MotionManager;
			manager.AccelerometerUpdateInterval = sensorSpeed.ToPlatform();
			manager.StartAccelerometerUpdates(Platform.GetCurrentQueue(), DataUpdated);
		}

		static void DataUpdated(CMAccelerometerData data, NSError error)
		{
			if (data == null)
				return;

			var field = data.Acceleration;
			var accelData = new AccelerometerData(field.X * -1, field.Y * -1, field.Z * -1);
			OnChanged(accelData);
		}

		internal static void PlatformStop() =>
			Platform.MotionManager?.StopAccelerometerUpdates();
	}
}
