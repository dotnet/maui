using System;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Devices
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/HapticFeedback.xml" path="Type[@FullName='Microsoft.Maui.Essentials.HapticFeedback']/Docs" />
	partial class HapticFeedbackImplementation : IHapticFeedback
	{
		public bool IsSupported
			=> throw ExceptionUtils.NotSupportedOrImplementedException;

		public void Perform(HapticFeedbackType type)
			=> throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
