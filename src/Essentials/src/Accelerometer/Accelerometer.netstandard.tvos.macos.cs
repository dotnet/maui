namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/Accelerometer.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Accelerometer']/Docs" />
	public static partial class Accelerometer
	{
		internal static bool IsSupported =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		static void PlatformStart(SensorSpeed sensorSpeed) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		static void PlatformStop() =>
			throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
