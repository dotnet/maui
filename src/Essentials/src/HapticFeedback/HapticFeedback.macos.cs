using System;
using System.Threading.Tasks;
using AppKit;

namespace Microsoft.Maui.Essentials
{
	public static partial class HapticFeedback
	{
		internal static bool IsSupported => true;

		static void PlatformPerform(HapticFeedbackType type)
		{
			if (type == HapticFeedbackType.LongPress)
				NSHapticFeedbackManager.DefaultPerformer.PerformFeedback(NSHapticFeedbackPattern.Generic, NSHapticFeedbackPerformanceTime.Default);
		}
	}
}
