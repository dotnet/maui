using System;

namespace Microsoft.Maui.Devices
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/Vibration.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Vibration']/Docs" />
	partial class VibrationImplementation : IVibration
	{
		public bool IsSupported
			=> throw ExceptionUtils.NotSupportedOrImplementedException;

		public void Vibrate() 
			=> throw ExceptionUtils.NotSupportedOrImplementedException;

		public void Vibrate(double duration) 
			=> throw ExceptionUtils.NotSupportedOrImplementedException;

		public void Vibrate(TimeSpan duration)
			=> throw ExceptionUtils.NotSupportedOrImplementedException;

		public void Cancel()
			=> throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
