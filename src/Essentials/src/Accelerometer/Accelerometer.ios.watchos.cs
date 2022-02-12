using CoreMotion;
using Foundation;

namespace Microsoft.Maui.Essentials.Implementations
{
	public partial class AccelerometerImplementation  : IAccelerometer
	{
		public bool IsSupported =>
			Platform.MotionManager?.AccelerometerAvailable ?? false;

		public bool IsMonitoring { get; set; }
	
		public void Start(SensorSpeed sensorSpeed)
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
			Accelerometer.OnChanged(accelData);
		}

		public void Stop() =>
			Platform.MotionManager?.StopAccelerometerUpdates();
	}
}
