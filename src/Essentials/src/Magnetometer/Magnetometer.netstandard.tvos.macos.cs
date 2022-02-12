namespace Microsoft.Maui.Essentials.Implementations
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/Magnetometer.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Magnetometer']/Docs" />
	public partial class MagnetometerImplementation : IMagnetometer
	{
		public bool IsSupported =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		public void Start(SensorSpeed sensorSpeed) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		public void Stop() =>
			throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
