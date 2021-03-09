using System;

namespace Microsoft.Maui.Essentials
{
	public static partial class HapticFeedback
	{
		public static void Perform(HapticFeedbackType type = HapticFeedbackType.Click)
		{
			if (!IsSupported)
				throw new FeatureNotSupportedException();
			PlatformPerform(type);
		}
	}
}
