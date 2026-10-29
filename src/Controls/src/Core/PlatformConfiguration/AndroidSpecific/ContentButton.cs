#nullable enable
using Microsoft.Maui.Graphics;
using FormsElement = Microsoft.Maui.Controls.ContentButton;

namespace Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific
{
	/// <summary>Controls Android-specific behavior for <see cref="FormsElement"/>.</summary>
	public static class ContentButton
	{
		/// <summary>Bindable property for the Android ripple color.</summary>
		public static readonly BindableProperty RippleColorProperty =
			BindableProperty.Create("RippleColor", typeof(Color), typeof(ContentButton), default(Color));

		/// <summary>Gets the Android ripple color.</summary>
		/// <param name="element">The content button.</param>
		/// <returns>The ripple color, or <see langword="null"/> to use the native theme color.</returns>
		public static Color? GetRippleColor(BindableObject element) =>
			(Color?)element.GetValue(RippleColorProperty);

		/// <summary>Sets the Android ripple color.</summary>
		/// <param name="element">The content button.</param>
		/// <param name="value">The ripple color, or <see langword="null"/> to use the native theme color.</param>
		public static void SetRippleColor(BindableObject element, Color? value) =>
			element.SetValue(RippleColorProperty, value);

		/// <summary>Gets the Android ripple color.</summary>
		/// <param name="config">The Android platform configuration.</param>
		/// <returns>The ripple color, or <see langword="null"/> to use the native theme color.</returns>
		public static Color? GetRippleColor(this IPlatformElementConfiguration<Android, FormsElement> config) =>
			GetRippleColor(config.Element);

		/// <summary>Sets the Android ripple color.</summary>
		/// <param name="config">The Android platform configuration.</param>
		/// <param name="value">The ripple color, or <see langword="null"/> to use the native theme color.</param>
		/// <returns>The platform configuration for fluent API chaining.</returns>
		public static IPlatformElementConfiguration<Android, FormsElement> SetRippleColor(
			this IPlatformElementConfiguration<Android, FormsElement> config,
			Color? value)
		{
			SetRippleColor(config.Element, value);
			return config;
		}
	}
}
