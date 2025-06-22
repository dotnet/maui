﻿#nullable enable

using Microsoft.Maui.Devices.Sensors;

namespace Microsoft.Maui.Devices
{
	/// <summary>
	/// The HapticFeedback API lets you control haptic feedback on the device.
	/// </summary>
	public interface IHapticFeedback : IDeviceCapabilities
	{
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
		/// Provides the default implementation for static usage of this API.
		/// </summary>
		public static IHapticFeedback Default =>
			defaultImplementation ??= new HapticFeedbackImplementation();

		internal static void SetDefault(IHapticFeedback? implementation) =>
			defaultImplementation = implementation;
	}
}
