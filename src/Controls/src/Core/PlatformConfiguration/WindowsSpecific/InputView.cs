#nullable disable
using System;

namespace Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific
{
	using FormsElement = Maui.Controls.InputView;

	/// <summary>Provides access to reading order detection on the Windows platform.</summary>
	public static class InputView
	{
		/// <summary>Bindable property for attached property <c>DetectReadingOrderFromContent</c>.</summary>
		public static readonly BindableProperty DetectReadingOrderFromContentProperty = BindableProperty.Create("DetectReadingOrderFromContent", typeof(bool), typeof(FormsElement), false);

		/// <summary>Sets whether reading order (LTR/RTL) is detected from content on Windows.</summary>
		/// <param name="element">The element to configure.</param>
		/// <param name="value"><see langword="true"/> to detect reading order from content.</param>
		public static void SetDetectReadingOrderFromContent(BindableObject element, bool value)
		{
			element.SetValue(DetectReadingOrderFromContentProperty, value);
		}

		/// <summary>Gets whether reading order (LTR/RTL) is detected from content on Windows.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <returns><see langword="true"/> if reading order detection is enabled.</returns>
		public static bool GetDetectReadingOrderFromContent(this IPlatformElementConfiguration<Windows, FormsElement> config)
		{
			return (bool)config.Element.GetValue(DetectReadingOrderFromContentProperty);
		}

		/// <summary>Gets whether reading order (LTR/RTL) is detected from content on Windows.</summary>
		/// <param name="element">The element to query.</param>
		/// <returns><see langword="true"/> if reading order detection is enabled.</returns>
		public static bool GetDetectReadingOrderFromContent(BindableObject element)
		{
			return (bool)element.GetValue(DetectReadingOrderFromContentProperty);
		}

		/// <summary>Sets whether reading order (LTR/RTL) is detected from content on Windows.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <param name="value"><see langword="true"/> to detect reading order from content.</param>
		/// <returns>The updated platform configuration.</returns>
		public static IPlatformElementConfiguration<Windows, FormsElement> SetDetectReadingOrderFromContent(
			this IPlatformElementConfiguration<Windows, FormsElement> config, bool value)
		{
			config.Element.SetValue(DetectReadingOrderFromContentProperty, value);
			return config;
		}
	}

}
