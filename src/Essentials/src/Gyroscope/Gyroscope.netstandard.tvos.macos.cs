using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Devices.Sensors
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/Gyroscope.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Gyroscope']/Docs" />
	public partial class GyroscopeImplementation : IGyroscope
	{
		bool PlatformIsSupported =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		void PlatformStart(SensorSpeed sensorSpeed) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		void PlatformStop() =>
			throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
