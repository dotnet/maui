#nullable enable
using CoreMotion;
using Foundation;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Devices.Sensors
{
	partial class MagnetometerImplementation : IMagnetometer
	{
		static CMMotionManager? motionManager;

		static CMMotionManager MotionManager =>
			motionManager ??= new CMMotionManager();

		bool PlatformIsSupported =>
			MotionManager.MagnetometerAvailable;

		void PlatformStart(SensorSpeed sensorSpeed)
		{
			MotionManager.MagnetometerUpdateInterval = sensorSpeed.ToPlatform();
			MotionManager.StartMagnetometerUpdates(NSOperationQueue.CurrentQueue ?? new NSOperationQueue(), DataUpdated);
		}

		void DataUpdated(CMMagnetometerData? data, NSError? error)
		{
			if (data == null)
				return;
#pragma warning disable CA1416 // https://github.com/xamarin/xamarin-macios/issues/14619
			var field = data.MagneticField;
			var magnetometerData = new MagnetometerData(field.X, field.Y, field.Z);
#pragma warning restore CA1416
			RaiseReadingChanged(magnetometerData);
		}

		void PlatformStop() =>
			MotionManager.StopMagnetometerUpdates();
	}
}
