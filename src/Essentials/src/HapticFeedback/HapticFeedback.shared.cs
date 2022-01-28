using System;

namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/HapticFeedback.xml" path="Type[@FullName='Microsoft.Maui.Essentials.HapticFeedback']/Docs" />
	public static partial class HapticFeedback
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/HapticFeedback.xml" path="//Member[@MemberName='Perform']/Docs" />
		public static void Perform(HapticFeedbackType type = HapticFeedbackType.Click)
		{
			if (!IsSupported)
				throw new FeatureNotSupportedException();
			PlatformPerform(type);
		}
	}
}
