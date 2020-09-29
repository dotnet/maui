namespace Xamarin.Forms.PlatformConfiguration.TizenSpecific
{
	using System.ComponentModel;
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

		public static readonly BindableProperty OverlayContentProperty = BindableProperty.CreateAttached("OverlayContent", typeof(View), typeof(FormsElement), default(View));

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

		public static readonly BindablePropertyKey ActiveBezelInteractionElementPropertyKey = BindableProperty.CreateAttachedReadOnly("ActiveBezelInteractionElement", typeof(Element), typeof(FormsElement), default(Element));

		public static Element GetActiveBezelInteractionElement(BindableObject application)
		{
			return (Element)application.GetValue(ActiveBezelInteractionElementPropertyKey.BindableProperty);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static void SetActiveBezelInteractionElement(BindableObject application, Element value)
		{
			application.SetValue(ActiveBezelInteractionElementPropertyKey, value);
		}

		public static Element GetActiveBezelInteractionElement(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			return GetActiveBezelInteractionElement(config.Element);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static IPlatformElementConfiguration<Tizen, FormsElement> SetActiveBezelInteractionElement(this IPlatformElementConfiguration<Tizen, FormsElement> config, Element value)
		{
			SetActiveBezelInteractionElement(config.Element, value);
			return config;
		}
	}
}