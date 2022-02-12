namespace Microsoft.Maui.Essentials.Implementations
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/Accelerometer.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Accelerometer']/Docs" />
	public partial class AccelerometerImplementation : IAccelerometer
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
		

		public bool IsMonitoring
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

		public void Start(SensorSpeed sensorSpeed) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;
		public void Stop() =>
			throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
