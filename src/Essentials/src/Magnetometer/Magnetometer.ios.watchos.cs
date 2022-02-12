using CoreMotion;
using Foundation;

namespace Microsoft.Maui.Essentials.Implementations
{
	public partial class MagnetometerImplementation : IMagnetometer
	{
		public bool IsSupported =>
			Platform.MotionManager?.MagnetometerAvailable ?? false;

		public void Start(SensorSpeed sensorSpeed)
		{
			var manager = Platform.MotionManager;
			manager.MagnetometerUpdateInterval = sensorSpeed.ToPlatform();
			manager.StartMagnetometerUpdates(Platform.GetCurrentQueue(), DataUpdated);
		}

		static void DataUpdated(CMMagnetometerData data, NSError error)
		{
			if (data == null)
				return;

			var field = data.MagneticField;
			var magnetometerData = new MagnetometerData(field.X, field.Y, field.Z);
			Magnetometer.OnChanged(magnetometerData);
		}

		public void Stop() =>
			Platform.MotionManager?.StopMagnetometerUpdates();
	}
}
