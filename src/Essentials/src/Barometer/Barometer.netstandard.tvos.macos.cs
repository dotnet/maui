using System;

namespace Microsoft.Maui.Essentials.Implementations
{
	public class BarometerImplementation : IBarometer
	{
#pragma warning disable CS0067
		public event EventHandler<BarometerChangedEventArgs> ReadingChanged;
#pragma warning restore CS0067

		public SensorSpeed SensorSpeed
			=> throw ExceptionUtils.NotSupportedOrImplementedException;

		public bool IsSupported
			=> throw ExceptionUtils.NotSupportedOrImplementedException;

		public bool IsMonitoring
			=> throw ExceptionUtils.NotSupportedOrImplementedException;

		public void Start(SensorSpeed sensorSpeed) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		public void Stop() =>
			throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
