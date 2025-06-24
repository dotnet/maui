#nullable disable
#if IOS || MACCATALYST
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration;

namespace Microsoft.AspNetCore.Components.WebView.Maui.PlatformConfiguration.iOSSpecific
{
	using FormsElement = Microsoft.AspNetCore.Components.WebView.Maui.BlazorWebView;

	/// <summary>
	/// Platform-specific configuration for BlazorWebView on iOS.
	/// </summary>
	public static class BlazorWebView
	{
		/// <summary>Bindable property for <see cref="IsScrollBounceEnabled"/>.</summary>
		public static readonly BindableProperty IsScrollBounceEnabledProperty = BindableProperty.Create(nameof(IsScrollBounceEnabled), typeof(bool), typeof(BlazorWebView), true);

		/// <summary>
		/// Gets the value that indicates whether scroll bouncing (elastic scrolling) is enabled for the BlazorWebView.
		/// </summary>
		/// <param name="element">The BlazorWebView element.</param>
		/// <returns>true if scroll bouncing is enabled; otherwise, false.</returns>
		public static bool GetIsScrollBounceEnabled(BindableObject element)
		{
			return (bool)element.GetValue(IsScrollBounceEnabledProperty);
		}

		/// <summary>
		/// Sets the value that indicates whether scroll bouncing (elastic scrolling) is enabled for the BlazorWebView.
		/// </summary>
		/// <param name="element">The BlazorWebView element.</param>
		/// <param name="value">true to enable scroll bouncing; false to disable scroll bouncing.</param>
		public static void SetIsScrollBounceEnabled(BindableObject element, bool value)
		{
			element.SetValue(IsScrollBounceEnabledProperty, value);
		}

		/// <summary>
		/// Gets the value that indicates whether scroll bouncing (elastic scrolling) is enabled for the BlazorWebView.
		/// </summary>
		/// <param name="config">The platform-specific configuration.</param>
		/// <returns>true if scroll bouncing is enabled; otherwise, false.</returns>
		public static bool IsScrollBounceEnabled(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetIsScrollBounceEnabled(config.Element);
		}

		/// <summary>
		/// Sets the value that indicates whether scroll bouncing (elastic scrolling) is enabled for the BlazorWebView.
		/// </summary>
		/// <param name="config">The platform-specific configuration.</param>
		/// <param name="value">true to enable scroll bouncing; false to disable scroll bouncing.</param>
		/// <returns>The updated configuration object.</returns>
		public static IPlatformElementConfiguration<iOS, FormsElement> SetIsScrollBounceEnabled(this IPlatformElementConfiguration<iOS, FormsElement> config, bool value)
		{
			SetIsScrollBounceEnabled(config.Element, value);
			return config;
		}
	}
}
#endif