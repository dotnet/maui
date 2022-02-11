namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/Accelerometer.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Accelerometer']/Docs" />
	public partial class AccelerometerImpl
	{
		public bool IsSupported =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		private void PlatformStart(SensorSpeed sensorSpeed) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		private void PlatformStop() =>
			throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
