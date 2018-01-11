
namespace Xamarin.Forms.PlatformConfiguration.iOSSpecific
{
	using FormsElement = Forms.VisualElement;

	public static class VisualElement
	{
		public static readonly BindableProperty BlurEffectProperty =
			BindableProperty.Create("BlurEffect", typeof(BlurEffectStyle),
			typeof(VisualElement), BlurEffectStyle.None);

		public static BlurEffectStyle GetBlurEffect(BindableObject element)
		{
			return (BlurEffectStyle)element.GetValue(BlurEffectProperty);
		}

		public static void SetBlurEffect(BindableObject element, BlurEffectStyle value)
		{
			element.SetValue(BlurEffectProperty, value);
		}

		public static BlurEffectStyle GetBlurEffect(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetBlurEffect(config.Element);
		}

		public static IPlatformElementConfiguration<iOS, FormsElement> UseBlurEffect(this IPlatformElementConfiguration<iOS, FormsElement> config, BlurEffectStyle value)
		{
			SetBlurEffect(config.Element, value);
			return config;
		}

		#region IsLegacyColorModeEnabled

		public static readonly BindableProperty IsLegacyColorModeEnabledProperty =
			BindableProperty.CreateAttached("IsLegacyColorModeEnabled", typeof(bool),
				typeof(FormsElement), true);

		public static bool GetIsLegacyColorModeEnabled(BindableObject element)
		{
			return (bool)element.GetValue(IsLegacyColorModeEnabledProperty);
		}

		public static void SetIsLegacyColorModeEnabled(BindableObject element, bool value)
		{
			element.SetValue(IsLegacyColorModeEnabledProperty, value);
		}

		public static bool GetIsLegacyColorModeEnabled(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return (bool)config.Element.GetValue(IsLegacyColorModeEnabledProperty);
		}

		public static IPlatformElementConfiguration<iOS, FormsElement> SetIsLegacyColorModeEnabled(
			this IPlatformElementConfiguration<iOS, FormsElement> config, bool value)
		{
			config.Element.SetValue(IsLegacyColorModeEnabledProperty, value);
			return config;
		}

		#endregion
	}
}
