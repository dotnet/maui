#nullable enable
using Microsoft.Maui.Devices;

namespace Microsoft.Maui.Devices
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/HapticFeedback.xml" path="Type[@FullName='Microsoft.Maui.Essentials.HapticFeedback']/Docs" />
	public static partial class HapticFeedback
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/HapticFeedback.xml" path="//Member[@MemberName='Perform']/Docs" />
		public static void Perform(HapticFeedbackType type = HapticFeedbackType.Click) =>
			Current.Perform(type);

		public static IHapticFeedback Current => Devices.HapticFeedback.Default;
	}
}
