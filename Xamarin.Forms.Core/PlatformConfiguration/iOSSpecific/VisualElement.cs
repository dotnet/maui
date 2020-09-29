
namespace Xamarin.Forms.PlatformConfiguration.iOSSpecific
{
	using System;
	using FormsElement = Forms.VisualElement;

	public static class VisualElement
	{
		public static readonly BindableProperty BlurEffectProperty = BindableProperty.Create("BlurEffect", typeof(BlurEffectStyle), typeof(VisualElement), BlurEffectStyle.None);

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

		#region Shadow Settings

		public class ShadowEffect : RoutingEffect
		{
			public ShadowEffect() : base("Xamarin.ShadowEffect") { }
		}

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

		public static bool GetIsShadowEnabled(BindableObject element)
		{
			return (bool)element.GetValue(IsShadowEnabledProperty);
		}

		public static void SetIsShadowEnabled(BindableObject element, bool value)
		{
			element.SetValue(IsShadowEnabledProperty, value);
		}

		public static bool GetIsShadowEnabled(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetIsShadowEnabled(config.Element);
		}

		public static IPlatformElementConfiguration<iOS, FormsElement> SetIsShadowEnabled(this IPlatformElementConfiguration<iOS, FormsElement> config, bool value)
		{
			SetIsShadowEnabled(config.Element, value);
			return config;
		}

		public static readonly BindableProperty ShadowColorProperty =
			BindableProperty.Create("ShadowColor", typeof(Color),
			typeof(VisualElement), Color.Default);

		public static Color GetShadowColor(BindableObject element)
		{
			return (Color)element.GetValue(ShadowColorProperty);
		}

		public static void SetShadowColor(BindableObject element, Color value)
		{
			element.SetValue(ShadowColorProperty, value);
		}

		public static Color GetShadowColor(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetShadowColor(config.Element);
		}

		public static IPlatformElementConfiguration<iOS, FormsElement> SetShadowColor(this IPlatformElementConfiguration<iOS, FormsElement> config, Color value)
		{
			SetShadowColor(config.Element, value);
			return config;
		}

		public static readonly BindableProperty ShadowRadiusProperty =
			BindableProperty.Create("ShadowRadius", typeof(double),
			typeof(VisualElement), 10.0);

		public static double GetShadowRadius(BindableObject element)
		{
			return (double)element.GetValue(ShadowRadiusProperty);
		}

		public static void SetShadowRadius(BindableObject element, double value)
		{
			element.SetValue(ShadowRadiusProperty, value);
		}

		public static double GetShadowRadius(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetShadowRadius(config.Element);
		}

		public static IPlatformElementConfiguration<iOS, FormsElement> SetShadowRadius(this IPlatformElementConfiguration<iOS, FormsElement> config, double value)
		{
			SetShadowRadius(config.Element, value);
			return config;
		}

		public static readonly BindableProperty ShadowOffsetProperty =
		BindableProperty.Create("ShadowOffset", typeof(Size),
		typeof(VisualElement), Size.Zero);

		public static Size GetShadowOffset(BindableObject element)
		{
			return (Size)element.GetValue(ShadowOffsetProperty);
		}

		public static void SetShadowOffset(BindableObject element, Size value)
		{
			element.SetValue(ShadowOffsetProperty, value);
		}

		public static Size GetShadowOffset(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetShadowOffset(config.Element);
		}

		public static IPlatformElementConfiguration<iOS, FormsElement> SetShadowOffset(this IPlatformElementConfiguration<iOS, FormsElement> config, Size value)
		{
			SetShadowOffset(config.Element, value);
			return config;
		}

		public static readonly BindableProperty ShadowOpacityProperty =
		BindableProperty.Create("ShadowOpacity", typeof(double),
		typeof(VisualElement), 0.5);

		public static double GetShadowOpacity(BindableObject element)
		{
			return (double)element.GetValue(ShadowOpacityProperty);
		}

		public static void SetShadowOpacity(BindableObject element, double value)
		{
			element.SetValue(ShadowOpacityProperty, value);
		}

		public static double GetShadowOpacity(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetShadowOpacity(config.Element);
		}

		public static IPlatformElementConfiguration<iOS, FormsElement> SetShadowOpacity(this IPlatformElementConfiguration<iOS, FormsElement> config, double value)
		{
			SetShadowOpacity(config.Element, value);
			return config;
		}

		#endregion

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

		public static readonly BindableProperty CanBecomeFirstResponderProperty = BindableProperty.Create(nameof(CanBecomeFirstResponder), typeof(bool), typeof(VisualElement), false);

		public static bool GetCanBecomeFirstResponder(BindableObject element)
		{
			return (bool)element.GetValue(CanBecomeFirstResponderProperty);
		}

		public static void SetCanBecomeFirstResponder(BindableObject element, bool value)
		{
			element.SetValue(CanBecomeFirstResponderProperty, value);
		}

		public static bool CanBecomeFirstResponder(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetCanBecomeFirstResponder(config.Element);
		}

		public static IPlatformElementConfiguration<iOS, FormsElement> SetCanBecomeFirstResponder(this IPlatformElementConfiguration<iOS, FormsElement> config, bool value)
		{
			SetCanBecomeFirstResponder(config.Element, value);
			return config;
		}
	}
}