namespace Microsoft.Maui.Essentials
{
	public static partial class Compass
	{
		internal static bool IsSupported =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		internal static void PlatformStart(SensorSpeed sensorSpeed, bool applyLowPassFilter) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		internal static void PlatformStop() =>
			throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
