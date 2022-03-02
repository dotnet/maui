namespace Microsoft.Maui.Essentials.Implementations
{
	public partial class CompassImplementation : ICompass
	{
		bool PlatformIsSupported => throw ExceptionUtils.NotSupportedOrImplementedException;

		void PlatformStart(SensorSpeed sensorSpeed, bool applyLowPassFilter) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		void PlatformStop() =>
			throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
