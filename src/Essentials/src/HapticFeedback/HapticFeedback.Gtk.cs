using System;
using System.Threading.Tasks;

namespace Microsoft.Maui.Essentials
{
	public static partial class HapticFeedback
	{
		internal static bool IsSupported
			=> throw ExceptionUtils.NotSupportedOrImplementedException;

		static void PlatformPerform(HapticFeedbackType type)
			=> throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
