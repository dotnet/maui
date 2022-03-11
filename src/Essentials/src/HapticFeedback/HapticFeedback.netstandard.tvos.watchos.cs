using System;
using System.Threading.Tasks;

namespace Microsoft.Maui.Essentials.Implementations
{
	public partial class HapticFeedbackImplementation : IHapticFeedback
	/// <include file="../../docs/Microsoft.Maui.Essentials/HapticFeedback.xml" path="Type[@FullName='Microsoft.Maui.Essentials.HapticFeedback']/Docs" />
	{
		public bool IsSupported
			=> throw ExceptionUtils.NotSupportedOrImplementedException;

		public void Perform(HapticFeedbackType type)
			=> throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
