using System;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Devices.Sensors
{
	public partial class BarometerImplementation : IBarometer
	{
		void PlatformStart(SensorSpeed sensorSpeed)
			=> throw ExceptionUtils.NotSupportedOrImplementedException;

		void PlatformStop()
			=> throw ExceptionUtils.NotSupportedOrImplementedException;

		bool PlatformIsSupported
			=> throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
