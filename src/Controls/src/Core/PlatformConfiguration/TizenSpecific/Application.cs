#nullable disable
using System.ComponentModel;

namespace Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific
{
	using FormsElement = Maui.Controls.Application;

	/// <summary>Provides Tizen-specific platform configuration for application-level features.</summary>
	public static class Application
	{
		/// <summary>Bindable property for attached property <c>UseBezelInteraction</c>.</summary>
		public static readonly BindableProperty UseBezelInteractionProperty = BindableProperty.Create("UseBezelInteraction", typeof(bool), typeof(FormsElement), true);

		/// <summary>Gets the value that indicates whether bezel interaction is enabled.</summary>
		/// <param name="element">The element whose bezel interaction setting to get.</param>
		/// <returns><see langword="true"/> if bezel interaction is enabled; otherwise, <see langword="false"/>.</returns>
		public static bool GetUseBezelInteraction(BindableObject element)
		{
			return (bool)element.GetValue(UseBezelInteractionProperty);
		}

		/// <summary>Sets a value that indicates whether bezel interaction is enabled.</summary>
		/// <param name="element">The element whose bezel interaction setting to set.</param>
		/// <param name="value"><see langword="true"/> to enable bezel interaction; otherwise, <see langword="false"/>.</param>
		public static void SetUseBezelInteraction(BindableObject element, bool value)
		{
			element.SetValue(UseBezelInteractionProperty, value);
		}

		/// <summary>Gets the value that indicates whether bezel interaction is enabled.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <returns><see langword="true"/> if bezel interaction is enabled; otherwise, <see langword="false"/>.</returns>
		public static bool GetUseBezelInteraction(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			return GetUseBezelInteraction(config.Element);
		}

		/// <summary>Sets a value that indicates whether bezel interaction is enabled.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <param name="value"><see langword="true"/> to enable bezel interaction; otherwise, <see langword="false"/>.</param>
		/// <returns>The updated platform configuration.</returns>
		public static IPlatformElementConfiguration<Tizen, FormsElement> SetUseBezelInteraction(this IPlatformElementConfiguration<Tizen, FormsElement> config, bool value)
		{
			SetUseBezelInteraction(config.Element, value);
			return config;
		}

		/// <summary>Bindable property for attached property <c>OverlayContent</c>.</summary>
		public static readonly BindableProperty OverlayContentProperty = BindableProperty.CreateAttached("OverlayContent", typeof(View), typeof(FormsElement), default(View));

		/// <summary>Gets the overlay content view for the application.</summary>
		/// <param name="application">The application whose overlay content to get.</param>
		/// <returns>The overlay content view.</returns>
		public static View GetOverlayContent(BindableObject application)
		{
			return (View)application.GetValue(OverlayContentProperty);
		}

		/// <summary>Sets the overlay content view for the application.</summary>
		/// <param name="application">The application whose overlay content to set.</param>
		/// <param name="value">The overlay content view.</param>
		public static void SetOverlayContent(BindableObject application, View value)
		{
			application.SetValue(OverlayContentProperty, value);
		}

		/// <summary>Gets the overlay content view for the application.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <returns>The overlay content view.</returns>
		public static View GetOverlayContent(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			return GetOverlayContent(config.Element);
		}

		/// <summary>Sets the overlay content view for the application.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <param name="value">The overlay content view.</param>
		/// <returns>The updated platform configuration.</returns>
		public static IPlatformElementConfiguration<Tizen, FormsElement> SetOverlayContent(this IPlatformElementConfiguration<Tizen, FormsElement> config, View value)
		{
			SetOverlayContent(config.Element, value);
			return config;
		}

		/// <summary>Bindable property key for the read-only attached property <c>ActiveBezelInteractionElement</c>.</summary>
		public static readonly BindablePropertyKey ActiveBezelInteractionElementPropertyKey = BindableProperty.CreateAttachedReadOnly("ActiveBezelInteractionElement", typeof(Element), typeof(FormsElement), default(Element));

		/// <summary>Gets the element that currently handles bezel interaction.</summary>
		/// <param name="application">The application whose active bezel interaction element to get.</param>
		/// <returns>The element that handles bezel interaction.</returns>
		public static Element GetActiveBezelInteractionElement(BindableObject application)
		{
			return (Element)application.GetValue(ActiveBezelInteractionElementPropertyKey.BindableProperty);
		}

		/// <summary>Sets the element that handles bezel interaction.</summary>
		/// <param name="application">The application whose active bezel interaction element to set.</param>
		/// <param name="value">The element to handle bezel interaction.</param>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static void SetActiveBezelInteractionElement(BindableObject application, Element value)
		{
			application.SetValue(ActiveBezelInteractionElementPropertyKey, value);
		}

		/// <summary>Gets the element that currently handles bezel interaction.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <returns>The element that handles bezel interaction.</returns>
		public static Element GetActiveBezelInteractionElement(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			return GetActiveBezelInteractionElement(config.Element);
		}

		/// <summary>Sets the element that handles bezel interaction.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <param name="value">The element to handle bezel interaction.</param>
		/// <returns>The updated platform configuration.</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static IPlatformElementConfiguration<Tizen, FormsElement> SetActiveBezelInteractionElement(this IPlatformElementConfiguration<Tizen, FormsElement> config, Element value)
		{
			SetActiveBezelInteractionElement(config.Element, value);
			return config;
		}
	}
}
