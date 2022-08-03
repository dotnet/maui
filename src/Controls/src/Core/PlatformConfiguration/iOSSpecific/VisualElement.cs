
namespace Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific
{
	using System;
	using Microsoft.Maui.Graphics;
	using FormsElement = Maui.Controls.VisualElement;

	/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/VisualElement.xml" path="Type[@FullName='Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.VisualElement']/Docs" />
	public static class VisualElement
	{
		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/VisualElement.xml" path="//Member[@MemberName='BlurEffectProperty']/Docs" />
		public static readonly BindableProperty BlurEffectProperty = BindableProperty.Create("BlurEffect", typeof(BlurEffectStyle), typeof(VisualElement), BlurEffectStyle.None);

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/VisualElement.xml" path="//Member[@MemberName='GetBlurEffect'][1]/Docs" />
		public static BlurEffectStyle GetBlurEffect(BindableObject element)
		{
			return (BlurEffectStyle)element.GetValue(BlurEffectProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/VisualElement.xml" path="//Member[@MemberName='SetBlurEffect']/Docs" />
		public static void SetBlurEffect(BindableObject element, BlurEffectStyle value)
		{
			element.SetValue(BlurEffectProperty, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/VisualElement.xml" path="//Member[@MemberName='GetBlurEffect'][2]/Docs" />
		public static BlurEffectStyle GetBlurEffect(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetBlurEffect(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/VisualElement.xml" path="//Member[@MemberName='UseBlurEffect']/Docs" />
		public static IPlatformElementConfiguration<iOS, FormsElement> UseBlurEffect(this IPlatformElementConfiguration<iOS, FormsElement> config, BlurEffectStyle value)
		{
			SetBlurEffect(config.Element, value);
			return config;
		}

		#region Shadow Settings

		public class ShadowEffect : RoutingEffect
		{
			public ShadowEffect() : base("Microsoft.Maui.Controls.ShadowEffect") { }
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/VisualElement.xml" path="//Member[@MemberName='IsShadowEnabledProperty']/Docs" />
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

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/VisualElement.xml" path="//Member[@MemberName='GetIsShadowEnabled'][1]/Docs" />
		public static bool GetIsShadowEnabled(BindableObject element)
		{
			return (bool)element.GetValue(IsShadowEnabledProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/VisualElement.xml" path="//Member[@MemberName='SetIsShadowEnabled'][1]/Docs" />
		public static void SetIsShadowEnabled(BindableObject element, bool value)
		{
			element.SetValue(IsShadowEnabledProperty, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/VisualElement.xml" path="//Member[@MemberName='GetIsShadowEnabled'][2]/Docs" />
		public static bool GetIsShadowEnabled(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetIsShadowEnabled(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/VisualElement.xml" path="//Member[@MemberName='SetIsShadowEnabled'][2]/Docs" />
		public static IPlatformElementConfiguration<iOS, FormsElement> SetIsShadowEnabled(this IPlatformElementConfiguration<iOS, FormsElement> config, bool value)
		{
			SetIsShadowEnabled(config.Element, value);
			return config;
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/VisualElement.xml" path="//Member[@MemberName='ShadowColorProperty']/Docs" />
		public static readonly BindableProperty ShadowColorProperty =
			BindableProperty.Create("ShadowColor", typeof(Color),
			typeof(VisualElement), null);

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/VisualElement.xml" path="//Member[@MemberName='GetShadowColor'][1]/Docs" />
		public static Color GetShadowColor(BindableObject element)
		{
			return (Color)element.GetValue(ShadowColorProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/VisualElement.xml" path="//Member[@MemberName='SetShadowColor'][1]/Docs" />
		public static void SetShadowColor(BindableObject element, Color value)
		{
			element.SetValue(ShadowColorProperty, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/VisualElement.xml" path="//Member[@MemberName='GetShadowColor'][2]/Docs" />
		public static Color GetShadowColor(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetShadowColor(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/VisualElement.xml" path="//Member[@MemberName='SetShadowColor'][2]/Docs" />
		public static IPlatformElementConfiguration<iOS, FormsElement> SetShadowColor(this IPlatformElementConfiguration<iOS, FormsElement> config, Color value)
		{
			SetShadowColor(config.Element, value);
			return config;
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/VisualElement.xml" path="//Member[@MemberName='ShadowRadiusProperty']/Docs" />
		public static readonly BindableProperty ShadowRadiusProperty =
			BindableProperty.Create("ShadowRadius", typeof(double),
			typeof(VisualElement), 10.0);

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/VisualElement.xml" path="//Member[@MemberName='GetShadowRadius'][1]/Docs" />
		public static double GetShadowRadius(BindableObject element)
		{
			return (double)element.GetValue(ShadowRadiusProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/VisualElement.xml" path="//Member[@MemberName='SetShadowRadius'][1]/Docs" />
		public static void SetShadowRadius(BindableObject element, double value)
		{
			element.SetValue(ShadowRadiusProperty, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/VisualElement.xml" path="//Member[@MemberName='GetShadowRadius'][2]/Docs" />
		public static double GetShadowRadius(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetShadowRadius(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/VisualElement.xml" path="//Member[@MemberName='SetShadowRadius'][2]/Docs" />
		public static IPlatformElementConfiguration<iOS, FormsElement> SetShadowRadius(this IPlatformElementConfiguration<iOS, FormsElement> config, double value)
		{
			SetShadowRadius(config.Element, value);
			return config;
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/VisualElement.xml" path="//Member[@MemberName='ShadowOffsetProperty']/Docs" />
		public static readonly BindableProperty ShadowOffsetProperty =
		BindableProperty.Create("ShadowOffset", typeof(Size),
		typeof(VisualElement), Size.Zero);

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/VisualElement.xml" path="//Member[@MemberName='GetShadowOffset'][1]/Docs" />
		public static Size GetShadowOffset(BindableObject element)
		{
			return (Size)element.GetValue(ShadowOffsetProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/VisualElement.xml" path="//Member[@MemberName='SetShadowOffset'][1]/Docs" />
		public static void SetShadowOffset(BindableObject element, Size value)
		{
			element.SetValue(ShadowOffsetProperty, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/VisualElement.xml" path="//Member[@MemberName='GetShadowOffset'][2]/Docs" />
		public static Size GetShadowOffset(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetShadowOffset(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/VisualElement.xml" path="//Member[@MemberName='SetShadowOffset'][2]/Docs" />
		public static IPlatformElementConfiguration<iOS, FormsElement> SetShadowOffset(this IPlatformElementConfiguration<iOS, FormsElement> config, Size value)
		{
			SetShadowOffset(config.Element, value);
			return config;
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/VisualElement.xml" path="//Member[@MemberName='ShadowOpacityProperty']/Docs" />
		public static readonly BindableProperty ShadowOpacityProperty =
		BindableProperty.Create("ShadowOpacity", typeof(double),
		typeof(VisualElement), 0.5);

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/VisualElement.xml" path="//Member[@MemberName='GetShadowOpacity'][1]/Docs" />
		public static double GetShadowOpacity(BindableObject element)
		{
			return (double)element.GetValue(ShadowOpacityProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/VisualElement.xml" path="//Member[@MemberName='SetShadowOpacity'][1]/Docs" />
		public static void SetShadowOpacity(BindableObject element, double value)
		{
			element.SetValue(ShadowOpacityProperty, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/VisualElement.xml" path="//Member[@MemberName='GetShadowOpacity'][2]/Docs" />
		public static double GetShadowOpacity(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetShadowOpacity(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/VisualElement.xml" path="//Member[@MemberName='SetShadowOpacity'][2]/Docs" />
		public static IPlatformElementConfiguration<iOS, FormsElement> SetShadowOpacity(this IPlatformElementConfiguration<iOS, FormsElement> config, double value)
		{
			SetShadowOpacity(config.Element, value);
			return config;
		}

		#endregion

		#region IsLegacyColorModeEnabled

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/VisualElement.xml" path="//Member[@MemberName='IsLegacyColorModeEnabledProperty']/Docs" />
		public static readonly BindableProperty IsLegacyColorModeEnabledProperty =
			BindableProperty.CreateAttached("IsLegacyColorModeEnabled", typeof(bool),
				typeof(FormsElement), true);

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/VisualElement.xml" path="//Member[@MemberName='GetIsLegacyColorModeEnabled'][1]/Docs" />
		public static bool GetIsLegacyColorModeEnabled(BindableObject element)
		{
			return (bool)element.GetValue(IsLegacyColorModeEnabledProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/VisualElement.xml" path="//Member[@MemberName='SetIsLegacyColorModeEnabled'][1]/Docs" />
		public static void SetIsLegacyColorModeEnabled(BindableObject element, bool value)
		{
			element.SetValue(IsLegacyColorModeEnabledProperty, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/VisualElement.xml" path="//Member[@MemberName='GetIsLegacyColorModeEnabled'][2]/Docs" />
		public static bool GetIsLegacyColorModeEnabled(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return (bool)config.Element.GetValue(IsLegacyColorModeEnabledProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/VisualElement.xml" path="//Member[@MemberName='SetIsLegacyColorModeEnabled'][2]/Docs" />
		public static IPlatformElementConfiguration<iOS, FormsElement> SetIsLegacyColorModeEnabled(
			this IPlatformElementConfiguration<iOS, FormsElement> config, bool value)
		{
			config.Element.SetValue(IsLegacyColorModeEnabledProperty, value);
			return config;
		}

		#endregion

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/VisualElement.xml" path="//Member[@MemberName='CanBecomeFirstResponderProperty']/Docs" />
		public static readonly BindableProperty CanBecomeFirstResponderProperty = BindableProperty.Create(nameof(CanBecomeFirstResponder), typeof(bool), typeof(VisualElement), false);

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/VisualElement.xml" path="//Member[@MemberName='GetCanBecomeFirstResponder']/Docs" />
		public static bool GetCanBecomeFirstResponder(BindableObject element)
		{
			return (bool)element.GetValue(CanBecomeFirstResponderProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/VisualElement.xml" path="//Member[@MemberName='SetCanBecomeFirstResponder'][1]/Docs" />
		public static void SetCanBecomeFirstResponder(BindableObject element, bool value)
		{
			element.SetValue(CanBecomeFirstResponderProperty, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/VisualElement.xml" path="//Member[@MemberName='CanBecomeFirstResponder']/Docs" />
		public static bool CanBecomeFirstResponder(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetCanBecomeFirstResponder(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/VisualElement.xml" path="//Member[@MemberName='SetCanBecomeFirstResponder'][2]/Docs" />
		public static IPlatformElementConfiguration<iOS, FormsElement> SetCanBecomeFirstResponder(this IPlatformElementConfiguration<iOS, FormsElement> config, bool value)
		{
			SetCanBecomeFirstResponder(config.Element, value);
			return config;
		}
	}
}
