#nullable enable

namespace Microsoft.Maui.Devices
{
	/// <summary>
	/// The HapticFeedback API lets you control haptic feedback on the device.
	/// </summary>
	public interface IHapticFeedback
	{
		/// <summary>
		/// Gets a value indicating whether haptic feedback is supported on this device.
		/// </summary>
		bool IsSupported { get; }

		/// <summary>
		/// Perform a type of haptic feedback on the device.
		/// </summary>
		/// <param name="type">The haptic feedback type to perform.</param>
		void Perform(HapticFeedbackType type);
	}

	/// <summary>
	/// The HapticFeedback API lets you control haptic feedback on the device.
	/// </summary>
	public static class HapticFeedback
	{
		/// <summary>
		/// Perform a type of haptic feedback on the device.
		/// </summary>
		/// <param name="type">The haptic feedback type to perform.</param>
		public static void Perform(HapticFeedbackType type = HapticFeedbackType.Click) =>
			Default.Perform(type);

		static IHapticFeedback? defaultImplementation;

		/// <summary>
		/// Gets a value indicating whether haptic feedback is supported on this device.
		/// </summary>
		public static bool IsSupported
			=> Current.IsSupported;

		static IHapticFeedback Current => Devices.HapticFeedback.Default;

		/// <summary>
		/// Provides the default implementation for static usage of this API.
		/// </summary>
		public static IHapticFeedback Default =>
			defaultImplementation ??= new HapticFeedbackImplementation();

		/// <summary>
		/// Sets the default implementation of this API that is exposed as the static <see cref="Default"/> property.
		/// </summary>
		/// <param name="implementation">An instance that implements the API, or <see langword="null"/> to reset to the platform default.</param>
		public static void SetDefault(IHapticFeedback? implementation) =>
			defaultImplementation = implementation;
	}
}
