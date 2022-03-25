#nullable enable

namespace Microsoft.Maui.Devices
{
	public interface IHapticFeedback
	{
		bool IsSupported { get; }

		void Perform(HapticFeedbackType type);
	}

	public static partial class HapticFeedback
	{
		static IHapticFeedback? defaultImplementation;

		public static IHapticFeedback Default =>
			defaultImplementation ??= new HapticFeedbackImplementation();

		internal static void SetDefault(IHapticFeedback? implementation) =>
			defaultImplementation = implementation;
	}
}
