#nullable disable

namespace Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific
{
	using FormsElement = Maui.Controls.WebView;

	/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/MixedContentHandling.xml" path="Type[@FullName='Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.MixedContentHandling']/Docs/*" />
	public enum MixedContentHandling
	{
		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/MixedContentHandling.xml" path="//Member[@MemberName='AlwaysAllow']/Docs/*" />
		AlwaysAllow = 0,
		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/MixedContentHandling.xml" path="//Member[@MemberName='NeverAllow']/Docs/*" />
		NeverAllow = 1,
		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/MixedContentHandling.xml" path="//Member[@MemberName='CompatibilityMode']/Docs/*" />
		CompatibilityMode = 2
	}

	/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/WebView.xml" path="Type[@FullName='Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.WebView']/Docs/*" />
	public static class WebView
	{
		/// <summary>Bindable property for <see cref="MixedContentMode"/>.</summary>
		public static readonly BindableProperty MixedContentModeProperty = BindableProperty.Create("MixedContentMode", typeof(MixedContentHandling), typeof(WebView), MixedContentHandling.NeverAllow);

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/WebView.xml" path="//Member[@MemberName='GetMixedContentMode']/Docs/*" />
		public static MixedContentHandling GetMixedContentMode(BindableObject element)
		{
			return (MixedContentHandling)element.GetValue(MixedContentModeProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/WebView.xml" path="//Member[@MemberName='SetMixedContentMode'][1]/Docs/*" />
		public static void SetMixedContentMode(BindableObject element, MixedContentHandling value)
		{
			element.SetValue(MixedContentModeProperty, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/WebView.xml" path="//Member[@MemberName='MixedContentMode']/Docs/*" />
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

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/WebView.xml" path="//Member[@MemberName='GetEnableZoomControls']/Docs/*" />
		public static bool GetEnableZoomControls(FormsElement element)
		{
			return (bool)element.GetValue(EnableZoomControlsProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/WebView.xml" path="//Member[@MemberName='SetEnableZoomControls'][2]/Docs/*" />
		public static void SetEnableZoomControls(FormsElement element, bool value)
		{
			element.SetValue(EnableZoomControlsProperty, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/WebView.xml" path="//Member[@MemberName='EnableZoomControls']/Docs/*" />
		public static void EnableZoomControls(this IPlatformElementConfiguration<Android, FormsElement> config, bool value)
		{
			SetEnableZoomControls(config.Element, value);
		}
		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/WebView.xml" path="//Member[@MemberName='ZoomControlsEnabled']/Docs/*" />
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

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/WebView.xml" path="//Member[@MemberName='GetDisplayZoomControls']/Docs/*" />
		public static bool GetDisplayZoomControls(FormsElement element)
		{
			return (bool)element.GetValue(DisplayZoomControlsProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/WebView.xml" path="//Member[@MemberName='SetDisplayZoomControls'][2]/Docs/*" />
		public static void SetDisplayZoomControls(FormsElement element, bool value)
		{
			element.SetValue(DisplayZoomControlsProperty, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/WebView.xml" path="//Member[@MemberName='DisplayZoomControls']/Docs/*" />
		public static void DisplayZoomControls(this IPlatformElementConfiguration<Android, FormsElement> config, bool value)
		{
			SetDisplayZoomControls(config.Element, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/WebView.xml" path="//Member[@MemberName='ZoomControlsDisplayed']/Docs/*" />
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
