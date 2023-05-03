#nullable disable
using System.ComponentModel;

namespace Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific
{
	using FormsElement = Maui.Controls.Application;

	/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/Application.xml" path="Type[@FullName='Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.Application']/Docs/*" />
	public static class Application
	{
		/// <summary>Bindable property for attached property <c>UseBezelInteraction</c>.</summary>
		public static readonly BindableProperty UseBezelInteractionProperty = BindableProperty.Create("UseBezelInteraction", typeof(bool), typeof(FormsElement), true);

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/Application.xml" path="//Member[@MemberName='GetUseBezelInteraction'][1]/Docs/*" />
		public static bool GetUseBezelInteraction(BindableObject element)
		{
			return (bool)element.GetValue(UseBezelInteractionProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/Application.xml" path="//Member[@MemberName='SetUseBezelInteraction'][1]/Docs/*" />
		public static void SetUseBezelInteraction(BindableObject element, bool value)
		{
			element.SetValue(UseBezelInteractionProperty, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/Application.xml" path="//Member[@MemberName='GetUseBezelInteraction'][2]/Docs/*" />
		public static bool GetUseBezelInteraction(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			return GetUseBezelInteraction(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/Application.xml" path="//Member[@MemberName='SetUseBezelInteraction'][2]/Docs/*" />
		public static IPlatformElementConfiguration<Tizen, FormsElement> SetUseBezelInteraction(this IPlatformElementConfiguration<Tizen, FormsElement> config, bool value)
		{
			SetUseBezelInteraction(config.Element, value);
			return config;
		}

		/// <summary>Bindable property for attached property <c>OverlayContent</c>.</summary>
		public static readonly BindableProperty OverlayContentProperty = BindableProperty.CreateAttached("OverlayContent", typeof(View), typeof(FormsElement), default(View));

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/Application.xml" path="//Member[@MemberName='GetOverlayContent'][1]/Docs/*" />
		public static View GetOverlayContent(BindableObject application)
		{
			return (View)application.GetValue(OverlayContentProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/Application.xml" path="//Member[@MemberName='SetOverlayContent'][1]/Docs/*" />
		public static void SetOverlayContent(BindableObject application, View value)
		{
			application.SetValue(OverlayContentProperty, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/Application.xml" path="//Member[@MemberName='GetOverlayContent'][2]/Docs/*" />
		public static View GetOverlayContent(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			return GetOverlayContent(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/Application.xml" path="//Member[@MemberName='SetOverlayContent'][2]/Docs/*" />
		public static IPlatformElementConfiguration<Tizen, FormsElement> SetOverlayContent(this IPlatformElementConfiguration<Tizen, FormsElement> config, View value)
		{
			SetOverlayContent(config.Element, value);
			return config;
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/Application.xml" path="//Member[@MemberName='ActiveBezelInteractionElementPropertyKey']/Docs/*" />
		public static readonly BindablePropertyKey ActiveBezelInteractionElementPropertyKey = BindableProperty.CreateAttachedReadOnly("ActiveBezelInteractionElement", typeof(Element), typeof(FormsElement), default(Element));

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/Application.xml" path="//Member[@MemberName='GetActiveBezelInteractionElement'][1]/Docs/*" />
		public static Element GetActiveBezelInteractionElement(BindableObject application)
		{
			return (Element)application.GetValue(ActiveBezelInteractionElementPropertyKey.BindableProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/Application.xml" path="//Member[@MemberName='SetActiveBezelInteractionElement'][1]/Docs/*" />
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static void SetActiveBezelInteractionElement(BindableObject application, Element value)
		{
			application.SetValue(ActiveBezelInteractionElementPropertyKey, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/Application.xml" path="//Member[@MemberName='GetActiveBezelInteractionElement'][2]/Docs/*" />
		public static Element GetActiveBezelInteractionElement(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			return GetActiveBezelInteractionElement(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/Application.xml" path="//Member[@MemberName='SetActiveBezelInteractionElement'][2]/Docs/*" />
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static IPlatformElementConfiguration<Tizen, FormsElement> SetActiveBezelInteractionElement(this IPlatformElementConfiguration<Tizen, FormsElement> config, Element value)
		{
			SetActiveBezelInteractionElement(config.Element, value);
			return config;
		}
	}
}
