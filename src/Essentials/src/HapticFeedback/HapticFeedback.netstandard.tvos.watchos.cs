using System;
using System.Threading.Tasks;

namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/HapticFeedback.xml" path="Type[@FullName='Microsoft.Maui.Essentials.HapticFeedback']/Docs" />
	public static partial class HapticFeedback
	{
		internal static bool IsSupported
			=> throw ExceptionUtils.NotSupportedOrImplementedException;

		static void PlatformPerform(HapticFeedbackType type)
			=> throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
