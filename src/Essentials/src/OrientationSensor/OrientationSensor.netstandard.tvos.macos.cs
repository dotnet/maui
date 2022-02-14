namespace Microsoft.Maui.Essentials.Implementations
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/OrientationSensor.xml" path="Type[@FullName='Microsoft.Maui.Essentials.OrientationSensor']/Docs" />
	public partial class OrientationSensorImplementation : IOrientationSensor
	{
		public bool IsSupported =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		public void Start(SensorSpeed sensorSpeed) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		public void Stop() =>
			throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
