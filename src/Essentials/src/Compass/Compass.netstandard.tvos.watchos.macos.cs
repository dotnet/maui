namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/Compass.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Compass']/Docs" />
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
