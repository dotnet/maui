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

		public static readonly BindableProperty OverlayContentProperty
		   = BindableProperty.CreateAttached("OverlayContent", typeof(View), typeof(FormsElement), default(View));

		public static View GetOverlayContent(BindableObject application)
		{
			return (View)application.GetValue(OverlayContentProperty);
		}

		public static void SetOverlayContent(BindableObject application, View value)
		{
			application.SetValue(OverlayContentProperty, value);
		}

		public static View GetOverlayContent(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			return GetOverlayContent(config.Element);
		}

		public static IPlatformElementConfiguration<Tizen, FormsElement> SetOverlayContent(this IPlatformElementConfiguration<Tizen, FormsElement> config, View value)
		{
			SetOverlayContent(config.Element, value);
			return config;
		}
	}
}
