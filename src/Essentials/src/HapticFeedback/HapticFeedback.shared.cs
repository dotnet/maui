#nullable enable

namespace Microsoft.Maui.Devices
{
	public interface IHapticFeedback
	{
		bool IsSupported { get; }

		void Perform(HapticFeedbackType type);
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/HapticFeedback.xml" path="Type[@FullName='Microsoft.Maui.Essentials.HapticFeedback']/Docs" />
	public static class HapticFeedback
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/HapticFeedback.xml" path="//Member[@MemberName='Perform']/Docs" />
		public static void Perform(HapticFeedbackType type = HapticFeedbackType.Click) =>
			Current.Perform(type);

		public static IHapticFeedback Current => Devices.HapticFeedback.Default;

		static IHapticFeedback? defaultImplementation;

		public static IHapticFeedback Default =>
			defaultImplementation ??= new HapticFeedbackImplementation();

		internal static void SetDefault(IHapticFeedback? implementation) =>
			defaultImplementation = implementation;
	}
}
