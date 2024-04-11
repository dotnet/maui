#nullable disable

namespace Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific
{
	using System;
	using Microsoft.Maui.Graphics;
	using FormsElement = Maui.Controls.VisualElement;

	/// <summary>
	/// Provides access to the blur effect, shadow effect, and legacy color mode on the iOS platform.
	/// </summary>
	public static class VisualElement
	{
		/// <summary>Bindable property for attached property <c>BlurEffect</c>.</summary>
		public static readonly BindableProperty BlurEffectProperty = BindableProperty.Create("BlurEffect", typeof(BlurEffectStyle), typeof(VisualElement), BlurEffectStyle.None);

		/// <summary>
		/// Returns a value that controls which, if any, blur effect is applied.
		/// </summary>
		/// <param name="element">The platform specific element on which to perform the operation.</param>
		/// <returns>A value that controls which, if any, blur effect is applied.</returns>
		public static BlurEffectStyle GetBlurEffect(BindableObject element)
		{
			return (BlurEffectStyle)element.GetValue(BlurEffectProperty);
		}

		/// <summary>
		/// Sets a value that controls which, if any, blur effect is applied.
		/// </summary>
		/// <param name="element">The platform specific element on which to perform the operation.</param>
		/// <param name="value">The new property value to assign.</param>
		public static void SetBlurEffect(BindableObject element, BlurEffectStyle value)
		{
			element.SetValue(BlurEffectProperty, value);
		}

		/// <summary>
		/// Returns a value that controls which, if any, blur effect is applied.
		/// </summary>
		/// <param name="config">The platform specific configuration that contains the element on which to perform the operation.</param>
		/// <returns>A value that controls which, if any, blur effect is applied.</returns>
		public static BlurEffectStyle GetBlurEffect(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetBlurEffect(config.Element);
		}

		/// <summary>
		/// Sets the blur effect to use.
		/// </summary>
		/// <param name="config">The platform specific configuration that contains the element on which to perform the operation.</param>
		/// <param name="value">The new property value to assign.</param>
		/// <returns>The updated configuration object on which developers can make successive method calls.</returns>
		public static IPlatformElementConfiguration<iOS, FormsElement> UseBlurEffect(this IPlatformElementConfiguration<iOS, FormsElement> config, BlurEffectStyle value)
		{
			SetBlurEffect(config.Element, value);
			return config;
		}

		#region Shadow Settings

		/// <summary>
		/// Represents a shadow effect that can be applied to iOS controls.
		/// </summary>
		public class ShadowEffect : RoutingEffect
		{
			public ShadowEffect() : base("Microsoft.Maui.Controls.ShadowEffect") { }
		}

		/// <summary>Bindable property for attached property <c>IsShadowEnabled</c>.</summary>
		public static readonly BindableProperty IsShadowEnabledProperty =
			BindableProperty.Create("IsShadowEnabled", typeof(bool),
			typeof(VisualElement), false, propertyChanged: OnIsShadowEnabledChanged);

		static void OnIsShadowEnabledChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var visualElement = bindable as FormsElement;
			var enabled = (bool)newValue;
			if (enabled)
			{
				visualElement.Effects.Add(new ShadowEffect());
			}
			else
			{
				foreach (var effect in visualElement.Effects)
				{
					if (effect is ShadowEffect)
					{
						visualElement.Effects.Remove(effect);
						break;
					}
				}
			}
		}

		/// <summary>
		/// Gets whether or not the shadow effect is enabled.
		/// </summary>
		/// <param name="element">The platform specific element on which to perform the operation.</param>
		/// <returns><see langword="true" /> if the shadow effect is enabled. Otherwise, <see langword="false" />.</returns>
		public static bool GetIsShadowEnabled(BindableObject element)
		{
			return (bool)element.GetValue(IsShadowEnabledProperty);
		}

		/// <summary>
		/// Sets whether or not the shadow effect is enabled.
		/// </summary>
		/// <param name="element">The platform specific element on which to perform the operation.</param>
		/// <param name="value"><see langword="true" /> to enable the shadow. Otherwise, <see langword="false" />.</param>
		public static void SetIsShadowEnabled(BindableObject element, bool value)
		{
			element.SetValue(IsShadowEnabledProperty, value);
		}

		/// <summary>
		/// Gets whether or not the shadow effect is enabled.
		/// </summary>
		/// <param name="config">The platform specific configuration that contains the element on which to perform the operation.</param>
		/// <returns><see langword="true" /> if the shadow effect is enabled. Otherwise, <see langword="false" />.</returns>
		public static bool GetIsShadowEnabled(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetIsShadowEnabled(config.Element);
		}

		/// <summary>
		/// Sets whether or not the shadow effect is enabled.
		/// </summary>
		/// <param name="config">The platform specific configuration that contains the element on which to perform the operation.</param>
		/// <param name="value"><see langword="true" /> to enable the shadow. Otherwise, <see langword="false" />.</param>
		/// <returns>A fluent object on which the developer may make more method calls.</returns>
		public static IPlatformElementConfiguration<iOS, FormsElement> SetIsShadowEnabled(this IPlatformElementConfiguration<iOS, FormsElement> config, bool value)
		{
			SetIsShadowEnabled(config.Element, value);
			return config;
		}

		/// <summary>Bindable property for attached property <c>ShadowColor</c>.</summary>
		public static readonly BindableProperty ShadowColorProperty =
			BindableProperty.Create("ShadowColor", typeof(Color),
			typeof(VisualElement), null);

		/// <summary>
		/// Gets the current shadow color.
		/// </summary>
		/// <param name="element">The platform specific element on which to perform the operation.</param>
		/// <returns>The current shadow color.</returns>
		public static Color GetShadowColor(BindableObject element)
		{
			return (Color)element.GetValue(ShadowColorProperty);
		}

		/// <summary>
		/// Sets the shadow color.
		/// </summary>
		/// <param name="element">The platform specific element on which to perform the operation.</param>
		/// <param name="value">The new shadow color value.</param>
		public static void SetShadowColor(BindableObject element, Color value)
		{
			element.SetValue(ShadowColorProperty, value);
		}

		/// <summary>
		/// Gets the current shadow color.
		/// </summary>
		/// <param name="config">The platform specific configuration that contains the element on which to perform the operation.</param>
		/// <returns>The current shadow color.</returns>
		public static Color GetShadowColor(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetShadowColor(config.Element);
		}

		/// <summary>
		/// Sets the shadow color.
		/// </summary>
		/// <param name="config">The platform specific configuration that contains the element on which to perform the operation.</param>
		/// <param name="value">The new shadow color value.</param>
		/// <returns>A fluent object on which the developer may make further method calls.</returns>
		public static IPlatformElementConfiguration<iOS, FormsElement> SetShadowColor(this IPlatformElementConfiguration<iOS, FormsElement> config, Color value)
		{
			SetShadowColor(config.Element, value);
			return config;
		}

		/// <summary>Bindable property for attached property <c>ShadowRadius</c>.</summary>
		public static readonly BindableProperty ShadowRadiusProperty =
			BindableProperty.Create("ShadowRadius", typeof(double),
			typeof(VisualElement), 10.0);

		/// <summary>
		/// Gets the current shadow radius.
		/// </summary>
		/// <param name="element">The platform specific element on which to perform the operation.</param>
		/// <returns>The current shadow radius.</returns>
		public static double GetShadowRadius(BindableObject element)
		{
			return (double)element.GetValue(ShadowRadiusProperty);
		}

		/// <summary>
		/// Sets the shadow radius.
		/// </summary>
		/// <param name="element">The platform specific element on which to perform the operation.</param>
		/// <param name="value">The new shadow radius value.</param>
		public static void SetShadowRadius(BindableObject element, double value)
		{
			element.SetValue(ShadowRadiusProperty, value);
		}

		/// <summary>
		/// Gets the current shadow radius.
		/// </summary>
		/// <param name="config">The platform specific configuration that contains the element on which to perform the operation.</param>
		/// <returns>The current shadow radius.</returns>
		public static double GetShadowRadius(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetShadowRadius(config.Element);
		}

		/// <summary>
		/// Sets the shadow radius.
		/// </summary>
		/// <param name="config">The platform specific configuration that contains the element on which to perform the operation.</param>
		/// <param name="value">The new shadow radius value.</param>
		/// <returns>A fluent object on which the developer may make further method calls.</returns>
		public static IPlatformElementConfiguration<iOS, FormsElement> SetShadowRadius(this IPlatformElementConfiguration<iOS, FormsElement> config, double value)
		{
			SetShadowRadius(config.Element, value);
			return config;
		}

		/// <summary>Bindable property for attached property <c>ShadowOffset</c>.</summary>
		public static readonly BindableProperty ShadowOffsetProperty =
		BindableProperty.Create("ShadowOffset", typeof(Size),
		typeof(VisualElement), Size.Zero);

		/// <summary>
		/// Gets the current shadow offset.
		/// </summary>
		/// <param name="element">The platform specific element on which to perform the operation.</param>
		/// <returns>The current shadow offset.</returns>
		public static Size GetShadowOffset(BindableObject element)
		{
			return (Size)element.GetValue(ShadowOffsetProperty);
		}

		/// <summary>
		/// Sets the shadow offset.
		/// </summary>
		/// <param name="element">The platform specific element on which to perform the operation.</param>
		/// <param name="value">The new shadow offset value.</param>
		public static void SetShadowOffset(BindableObject element, Size value)
		{
			element.SetValue(ShadowOffsetProperty, value);
		}

		/// <summary>
		/// Gets the current shadow offset.
		/// </summary>
		/// <param name="config">The platform specific configuration that contains the element on which to perform the operation.</param>
		/// <returns>The current shadow offset.</returns>
		public static Size GetShadowOffset(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetShadowOffset(config.Element);
		}

		/// <summary>
		/// Sets the shadow offset.
		/// </summary>
		/// <param name="config">The platform specific configuration that contains the element on which to perform the operation.</param>
		/// <param name="value">The new shadow offset value.</param>
		/// <returns>A fluent object on which the developer may make further method calls.</returns>
		public static IPlatformElementConfiguration<iOS, FormsElement> SetShadowOffset(this IPlatformElementConfiguration<iOS, FormsElement> config, Size value)
		{
			SetShadowOffset(config.Element, value);
			return config;
		}

		/// <summary>Bindable property for attached property <c>ShadowOpacity</c>.</summary>
		public static readonly BindableProperty ShadowOpacityProperty =
		BindableProperty.Create("ShadowOpacity", typeof(double),
		typeof(VisualElement), 0.5);

		/// <summary>
		/// Gets the current shadow opacity.
		/// </summary>
		/// <param name="element">The platform specific element on which to perform the operation.</param>
		/// <returns>The current shadow opacity.</returns>
		public static double GetShadowOpacity(BindableObject element)
		{
			return (double)element.GetValue(ShadowOpacityProperty);
		}

		/// <summary>
		/// Sets the shadow opacity.
		/// </summary>
		/// <param name="element">The platform specific element on which to perform the operation.</param>
		/// <param name="value">The new shadow opacity value.</param>
		public static void SetShadowOpacity(BindableObject element, double value)
		{
			element.SetValue(ShadowOpacityProperty, value);
		}

		/// <summary>
		/// The shadow opacity.
		/// </summary>
		/// <param name="config">The platform specific configuration that contains the element on which to perform the operation.</param>
		/// <returns>The current shadow opacity.</returns>
		public static double GetShadowOpacity(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetShadowOpacity(config.Element);
		}

		/// <summary>
		/// Sets the shadow opacity.
		/// </summary>
		/// <param name="config">The platform specific configuration that contains the element on which to perform the operation.</param>
		/// <param name="value">The new shadow opacity value.</param>
		/// <returns>A fluent object on which the developer may make further method calls.</returns>
		public static IPlatformElementConfiguration<iOS, FormsElement> SetShadowOpacity(this IPlatformElementConfiguration<iOS, FormsElement> config, double value)
		{
			SetShadowOpacity(config.Element, value);
			return config;
		}

		#endregion

		#region IsLegacyColorModeEnabled

		/// <summary>Bindable property for attached property <c>IsLegacyColorModeEnabled</c>.</summary>
		public static readonly BindableProperty IsLegacyColorModeEnabledProperty =
			BindableProperty.CreateAttached("IsLegacyColorModeEnabled", typeof(bool),
				typeof(FormsElement), true);

		/// <summary>
		/// Returns whether or not the legacy color mode is enabled.
		/// </summary>
		/// <param name="element">The platform specific element on which to perform the operation.</param>
		/// <returns><see langword="true" /> if the legacy color mode is enabled. Otherwise, <see langword="false" />.</returns>
		public static bool GetIsLegacyColorModeEnabled(BindableObject element)
		{
			return (bool)element.GetValue(IsLegacyColorModeEnabledProperty);
		}

		/// <summary>
		/// Sets whether the legacy color mode is enabled.
		/// </summary>
		/// <param name="element">The platform specific element on which to perform the operation.</param>
		/// <param name="value"><see langword="true" /> to enable legacy color mode. Otherwise, <see langword="false" />.</param>
		public static void SetIsLegacyColorModeEnabled(BindableObject element, bool value)
		{
			element.SetValue(IsLegacyColorModeEnabledProperty, value);
		}

		/// <summary>
		/// Returns whether or not the legacy color mode is enabled.
		/// </summary>
		/// <param name="config">The platform specific configuration that contains the element on which to perform the operation.</param>
		/// <returns><see langword="true" /> if the legacy color mode is enabled. Otherwise, <see langword="false" />.</returns>
		public static bool GetIsLegacyColorModeEnabled(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return (bool)config.Element.GetValue(IsLegacyColorModeEnabledProperty);
		}

		/// <summary>
		/// Sets whether the legacy color mode is enabled.
		/// </summary>
		/// <param name="config">The platform specific configuration that contains the element on which to perform the operation.</param>
		/// <param name="value"><see langword="true" /> to enable legacy color mode. Otherwise, <see langword="false" />.</param>
		/// <returns>A fluent object on which the developer may make further method calls.</returns>
		public static IPlatformElementConfiguration<iOS, FormsElement> SetIsLegacyColorModeEnabled(
			this IPlatformElementConfiguration<iOS, FormsElement> config, bool value)
		{
			config.Element.SetValue(IsLegacyColorModeEnabledProperty, value);
			return config;
		}

		#endregion

		/// <summary>Bindable property for <see cref="CanBecomeFirstResponder"/>.</summary>
		public static readonly BindableProperty CanBecomeFirstResponderProperty = BindableProperty.Create(nameof(CanBecomeFirstResponder), typeof(bool), typeof(VisualElement), false);

		/// <summary>
		/// Gets whether this element can become the first responder to touch events, rather than the page containing the element.
		/// </summary>
		/// <param name="element">The platform specific element on which to perform the operation.</param>
		/// <returns><see langword="true" /> when this element can become first responder. Otherwise, <see langword="false" />.</returns>
		public static bool GetCanBecomeFirstResponder(BindableObject element)
		{
			return (bool)element.GetValue(CanBecomeFirstResponderProperty);
		}

		/// <summary>
		/// Sets whether this element can become the first responder to touch events, rather than the page containing the element.
		/// </summary>
		/// <param name="element">The platform specific element on which to perform the operation.</param>
		/// <param name="value"><see langword="true" /> to set this element as the first responder. Otherwise, <see langword="false" />.</param>
		public static void SetCanBecomeFirstResponder(BindableObject element, bool value)
		{
			element.SetValue(CanBecomeFirstResponderProperty, value);
		}

		/// <summary>
		/// Gets whether this element can become the first responder to touch events, rather than the page containing the element.
		/// </summary>
		/// <param name="config">The platform specific configuration that contains the element on which to perform the operation.</param>
		/// <returns><see langword="true" /> when this element can become first responder. Otherwise, <see langword="false" />.</returns>
		public static bool CanBecomeFirstResponder(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetCanBecomeFirstResponder(config.Element);
		}

		/// <summary>
		/// Sets whether this element can become the first responder to touch events, rather than the page containing the element.
		/// </summary>
		/// <param name="config">The platform specific configuration that contains the element on which to perform the operation.</param>
		/// <param name="value"><see langword="true" /> to set this element as the first responder. Otherwise, <see langword="false" />.</param>
		/// <returns>A fluent object on which the developer may make further method calls.</returns>
		public static IPlatformElementConfiguration<iOS, FormsElement> SetCanBecomeFirstResponder(this IPlatformElementConfiguration<iOS, FormsElement> config, bool value)
		{
			SetCanBecomeFirstResponder(config.Element, value);
			return config;
		}
	}
}
