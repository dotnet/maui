namespace Microsoft.Maui.Essentials.Implementations
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/Gyroscope.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Gyroscope']/Docs" />
	public partial class GyroscopeImplementation : IGyroscope
	{
		public bool IsSupported =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		public void Start(SensorSpeed sensorSpeed) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		public void Stop() =>
			throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
