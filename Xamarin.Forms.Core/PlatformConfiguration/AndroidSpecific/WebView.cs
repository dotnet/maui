
namespace Xamarin.Forms.PlatformConfiguration.AndroidSpecific
{
	using FormsElement = Forms.WebView;

	public enum MixedContentHandling
	{
		AlwaysAllow = 0,
		NeverAllow = 1,
		CompatibilityMode = 2
	}

	public static class WebView
	{
		public static readonly BindableProperty MixedContentModeProperty = BindableProperty.Create("MixedContentMode", typeof(MixedContentHandling), typeof(WebView), MixedContentHandling.NeverAllow);

		public static MixedContentHandling GetMixedContentMode(BindableObject element)
		{
			return (MixedContentHandling)element.GetValue(MixedContentModeProperty);
		}

		public static void SetMixedContentMode(BindableObject element, MixedContentHandling value)
		{
			element.SetValue(MixedContentModeProperty, value);
		}

		public static MixedContentHandling MixedContentMode(this IPlatformElementConfiguration<Android, FormsElement> config)
		{
			return GetMixedContentMode(config.Element);
		}

		public static IPlatformElementConfiguration<Android, FormsElement> SetMixedContentMode(this IPlatformElementConfiguration<Android, FormsElement> config, MixedContentHandling value)
		{
			SetMixedContentMode(config.Element, value);
			return config;
		}

		public static readonly BindableProperty EnableZoomControlsProperty = BindableProperty.Create("EnableZoomControls", typeof(bool), typeof(FormsElement), false);

		public static bool GetEnableZoomControls(FormsElement element)
		{
			return (bool)element.GetValue(EnableZoomControlsProperty);
		}

		public static void SetEnableZoomControls(FormsElement element, bool value)
		{
			element.SetValue(EnableZoomControlsProperty, value);
		}

		public static void EnableZoomControls(this IPlatformElementConfiguration<Android, FormsElement> config, bool value)
		{
			SetEnableZoomControls(config.Element, value);
		}
		public static bool ZoomControlsEnabled(this IPlatformElementConfiguration<Android, FormsElement> config)
		{
			return GetEnableZoomControls(config.Element);
		}

		public static IPlatformElementConfiguration<Android, FormsElement> SetEnableZoomControls(this IPlatformElementConfiguration<Android, FormsElement> config, bool value)
		{
			SetEnableZoomControls(config.Element, value);
			return config;
		}

		public static readonly BindableProperty DisplayZoomControlsProperty = BindableProperty.Create("DisplayZoomControls", typeof(bool), typeof(FormsElement), true);

		public static bool GetDisplayZoomControls(FormsElement element)
		{
			return (bool)element.GetValue(DisplayZoomControlsProperty);
		}

		public static void SetDisplayZoomControls(FormsElement element, bool value)
		{
			element.SetValue(DisplayZoomControlsProperty, value);
		}

		public static void DisplayZoomControls(this IPlatformElementConfiguration<Android, FormsElement> config, bool value)
		{
			SetDisplayZoomControls(config.Element, value);
		}

		public static bool ZoomControlsDisplayed(this IPlatformElementConfiguration<Android, FormsElement> config)
		{
			return GetDisplayZoomControls(config.Element);
		}

		public static IPlatformElementConfiguration<Android, FormsElement> SetDisplayZoomControls(this IPlatformElementConfiguration<Android, FormsElement> config, bool value)
		{
			SetDisplayZoomControls(config.Element, value);
			return config;
		}
	}
}