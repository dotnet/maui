
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
	}
}
