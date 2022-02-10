namespace Microsoft.Maui.Essentials.Implementations
{
	public partial class CompassImplementation : ICompass
	/// <include file="../../docs/Microsoft.Maui.Essentials/Compass.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Compass']/Docs" />
	{
		public bool IsSupported =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

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

		public void Start(SensorSpeed sensorSpeed, bool applyLowPassFilter) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		public void Stop() =>
			throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
