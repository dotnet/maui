namespace Microsoft.Maui.Essentials.Implementations
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/Barometer.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Barometer']/Docs" />
	public class BarometerImplementation : IBarometer
	{
		public bool IsSupported
		{
			get
			{
				throw ExceptionUtils.NotSupportedOrImplementedException;
			}

 			set
			{
				throw ExceptionUtils.NotSupportedOrImplementedException;
			}
		}

		public bool IsMonitoring { get; set; }

		public void Start(SensorSpeed sensorSpeed) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		public void Stop() =>
			throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
