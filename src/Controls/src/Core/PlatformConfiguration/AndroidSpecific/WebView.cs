#nullable disable

namespace Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific
{
	using FormsElement = Maui.Controls.WebView;

	/// <summary>Enumerates web view behaviors when handling mixed content.</summary>
	public enum MixedContentHandling
	{
		/// <summary>Allow all content, whether secure or insecure.</summary>
		AlwaysAllow = 0,
		/// <summary>Never allow insecure content when loading from a secure URL.</summary>
		NeverAllow = 1,
		/// <summary>Selectively allow both secure and insecure content in a way that is not controlled by the application developer.</summary>
		CompatibilityMode = 2
	}

	/// <summary>Controls the mixed content mode on web views on the Android platform.</summary>
	public static class WebView
	{
		/// <summary>Bindable property for <see cref="MixedContentMode"/>.</summary>
		public static readonly BindableProperty MixedContentModeProperty = BindableProperty.Create("MixedContentMode", typeof(MixedContentHandling), typeof(WebView), MixedContentHandling.NeverAllow);

		/// <summary>Returns the mixed content mode for the web view.</summary>
		/// <param name="element">The Android web view for which to get the loading behavior for content that is a mix of secure and insecure content.</param>
		public static MixedContentHandling GetMixedContentMode(BindableObject element)
		{
			return (MixedContentHandling)element.GetValue(MixedContentModeProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/WebView.xml" path="//Member[@MemberName='SetMixedContentMode'][1]/Docs/*" />
		public static void SetMixedContentMode(BindableObject element, MixedContentHandling value)
		{
			element.SetValue(MixedContentModeProperty, value);
		}

		/// <summary>Gets the mixed content loading behavior.</summary>
		/// <param name="config">The platform configuration for the Android web view for which to get the loading behavior for content that is a mix of secure and insecure content.</param>
		public static MixedContentHandling MixedContentMode(this IPlatformElementConfiguration<Android, FormsElement> config)
		{
			return GetMixedContentMode(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/WebView.xml" path="//Member[@MemberName='SetMixedContentMode'][2]/Docs/*" />
		public static IPlatformElementConfiguration<Android, FormsElement> SetMixedContentMode(this IPlatformElementConfiguration<Android, FormsElement> config, MixedContentHandling value)
		{
			SetMixedContentMode(config.Element, value);
			return config;
		}

		/// <summary>Bindable property for <see cref="EnableZoomControls"/>.</summary>
		public static readonly BindableProperty EnableZoomControlsProperty = BindableProperty.Create("EnableZoomControls", typeof(bool), typeof(FormsElement), false);

		/// <param name="element">The element on which to perform the operation.</param>
		public static bool GetEnableZoomControls(FormsElement element)
		{
			return (bool)element.GetValue(EnableZoomControlsProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/WebView.xml" path="//Member[@MemberName='SetEnableZoomControls'][2]/Docs/*" />
		public static void SetEnableZoomControls(FormsElement element, bool value)
		{
			element.SetValue(EnableZoomControlsProperty, value);
		}

		/// <param name="config">The platform configuration for the element on which to perform the operation.</param>
		/// <param name="value">The value to set.</param>
		public static void EnableZoomControls(this IPlatformElementConfiguration<Android, FormsElement> config, bool value)
		{
			SetEnableZoomControls(config.Element, value);
		}
		/// <param name="config">The platform configuration for the element on which to perform the operation.</param>
		public static bool ZoomControlsEnabled(this IPlatformElementConfiguration<Android, FormsElement> config)
		{
			return GetEnableZoomControls(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/WebView.xml" path="//Member[@MemberName='SetEnableZoomControls'][1]/Docs/*" />
		public static IPlatformElementConfiguration<Android, FormsElement> SetEnableZoomControls(this IPlatformElementConfiguration<Android, FormsElement> config, bool value)
		{
			SetEnableZoomControls(config.Element, value);
			return config;
		}

		/// <summary>Bindable property for <see cref="DisplayZoomControls"/>.</summary>
		public static readonly BindableProperty DisplayZoomControlsProperty = BindableProperty.Create("DisplayZoomControls", typeof(bool), typeof(FormsElement), true);

		/// <param name="element">The element on which to perform the operation.</param>
		public static bool GetDisplayZoomControls(FormsElement element)
		{
			return (bool)element.GetValue(DisplayZoomControlsProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/WebView.xml" path="//Member[@MemberName='SetDisplayZoomControls'][2]/Docs/*" />
		public static void SetDisplayZoomControls(FormsElement element, bool value)
		{
			element.SetValue(DisplayZoomControlsProperty, value);
		}

		/// <param name="config">The platform configuration for the element on which to perform the operation.</param>
		/// <param name="value">The value to set.</param>
		public static void DisplayZoomControls(this IPlatformElementConfiguration<Android, FormsElement> config, bool value)
		{
			SetDisplayZoomControls(config.Element, value);
		}

		/// <param name="config">The platform configuration for the element on which to perform the operation.</param>
		public static bool ZoomControlsDisplayed(this IPlatformElementConfiguration<Android, FormsElement> config)
		{
			return GetDisplayZoomControls(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/WebView.xml" path="//Member[@MemberName='SetDisplayZoomControls'][1]/Docs/*" />
		public static IPlatformElementConfiguration<Android, FormsElement> SetDisplayZoomControls(this IPlatformElementConfiguration<Android, FormsElement> config, bool value)
		{
			SetDisplayZoomControls(config.Element, value);
			return config;
		}

		/// <summary>
		/// Bindable property for controlling whether JavaScript is enabled in a Element.
		/// </summary>
		public static readonly BindableProperty JavaScriptEnabledProperty = BindableProperty.Create("JavaScriptEnabled", typeof(bool), typeof(FormsElement), true);

		/// <summary>
		/// Gets the value of the JavaScriptEnabled property for the specified WebView.
		/// </summary>
		/// <param name="element">The WebView from which to read the property value.</param>
		/// <returns>A boolean value indicating whether JavaScript is enabled.</returns>
		public static bool GetJavaScriptEnabled(FormsElement element)
		{
			return (bool)element.GetValue(JavaScriptEnabledProperty);
		}

		/// <summary>
		/// Sets the value of the JavaScriptEnabled property for the specified WebView.
		/// </summary>
		/// <param name="element">The WebView on which to set the property value.</param>
		/// <param name="value">The boolean value indicating whether JavaScript should be enabled.</param>
		public static void SetJavaScriptEnabled(FormsElement element, bool value)
		{
			element.SetValue(JavaScriptEnabledProperty, value);
		}

		/// <summary>
		/// Sets whether JavaScript is enabled for the specified Android platform configuration.
		/// </summary>
		/// <param name="config">The platform configuration to which this method applies.</param>
		/// <param name="value">The boolean value indicating whether JavaScript should be enabled.</param>
		public static void JavaScriptEnabled(this IPlatformElementConfiguration<Android, FormsElement> config, bool value)
		{
			SetJavaScriptEnabled(config.Element, value);
		}

		/// <summary>
		/// Determines whether JavaScript is enabled for the specified Android platform configuration.
		/// </summary>
		/// <param name="config">The platform configuration to check.</param>
		/// <returns>A boolean value indicating whether JavaScript is enabled.</returns>
		public static bool IsJavaScriptEnabled(this IPlatformElementConfiguration<Android, FormsElement> config)
		{
			return GetJavaScriptEnabled(config.Element);
		}

		/// <summary>
		/// Sets whether JavaScript is enabled for the specified Android platform configuration and returns the configuration.
		/// </summary>
		/// <param name="config">The platform configuration to which this method applies.</param>
		/// <param name="value">The boolean value indicating whether JavaScript should be enabled.</param>
		/// <returns>The platform configuration to support fluent API.</returns>
		public static IPlatformElementConfiguration<Android, FormsElement> SetJavaScriptEnabled(this IPlatformElementConfiguration<Android, FormsElement> config, bool value)
		{
			SetJavaScriptEnabled(config.Element, value);
			return config;
		}
	}
}
