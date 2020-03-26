namespace Xamarin.Forms.PlatformConfiguration.TizenSpecific
{
	using FormsElement = Forms.Application;

	public static class Application
	{
		public static readonly BindableProperty UseBezelInteractionProperty = BindableProperty.Create("UseBezelInteraction", typeof(bool), typeof(FormsElement), true);

		public static bool GetUseBezelInteraction(BindableObject element)
		{
			return (bool)element.GetValue(UseBezelInteractionProperty);
		}

		public static void SetUseBezelInteraction(BindableObject element, bool value)
		{
			element.SetValue(UseBezelInteractionProperty, value);
		}

		public static bool GetUseBezelInteraction(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			return GetUseBezelInteraction(config.Element);
		}

		public static IPlatformElementConfiguration<Tizen, FormsElement> SetUseBezelInteraction(this IPlatformElementConfiguration<Tizen, FormsElement> config, bool value)
		{
			SetUseBezelInteraction(config.Element, value);
			return config;
		}
	}
}
