
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
	}
}
