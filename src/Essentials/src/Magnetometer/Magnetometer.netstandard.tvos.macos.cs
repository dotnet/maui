namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/Magnetometer.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Magnetometer']/Docs" />
	public static partial class Magnetometer
	{
		internal static bool IsSupported =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		internal static void PlatformStart(SensorSpeed sensorSpeed) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		internal static void PlatformStop() =>
			throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
