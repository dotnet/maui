using System;

namespace Microsoft.Maui.Devices.Sensors
{
	partial class BarometerImplementation : IBarometer
	{
		void PlatformStart(SensorSpeed sensorSpeed)
			=> throw ExceptionUtils.NotSupportedOrImplementedException;

		void PlatformStop()
			=> throw ExceptionUtils.NotSupportedOrImplementedException;

		public bool IsSupported
			=> throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
