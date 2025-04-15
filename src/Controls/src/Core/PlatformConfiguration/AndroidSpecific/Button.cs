#nullable disable
namespace Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific
{
	using Microsoft.Maui.Graphics;
	using FormsElement = Maui.Controls.Button;

	/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/Button.xml" path="Type[@FullName='Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.Button']/Docs/*" />
	public static class Button
	{
		#region UseDefaultPadding
		/// <summary>Bindable property for <see cref="UseDefaultPadding"/>.</summary>
		public static readonly BindableProperty UseDefaultPaddingProperty = BindableProperty.Create("UseDefaultPadding", typeof(bool), typeof(Button), false);

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/Button.xml" path="//Member[@MemberName='GetUseDefaultPadding']/Docs/*" />
		public static bool GetUseDefaultPadding(BindableObject element)
		{
			return (bool)element.GetValue(UseDefaultPaddingProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/Button.xml" path="//Member[@MemberName='SetUseDefaultPadding'][1]/Docs/*" />
		public static void SetUseDefaultPadding(BindableObject element, bool value)
		{
			element.SetValue(UseDefaultPaddingProperty, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/Button.xml" path="//Member[@MemberName='UseDefaultPadding']/Docs/*" />
		public static bool UseDefaultPadding(this IPlatformElementConfiguration<Android, FormsElement> config)
		{
			return GetUseDefaultPadding(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/Button.xml" path="//Member[@MemberName='SetUseDefaultPadding'][2]/Docs/*" />
		public static IPlatformElementConfiguration<Android, FormsElement> SetUseDefaultPadding(this IPlatformElementConfiguration<Android, FormsElement> config, bool value)
		{
			SetUseDefaultPadding(config.Element, value);
			return config;
		}
		#endregion

		#region UseDefaultShadow
		/// <summary>Bindable property for <see cref="UseDefaultShadow"/>.</summary>
		public static readonly BindableProperty UseDefaultShadowProperty = BindableProperty.Create("UseDefaultShadow", typeof(bool), typeof(Button), false);

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/Button.xml" path="//Member[@MemberName='GetUseDefaultShadow']/Docs/*" />
		public static bool GetUseDefaultShadow(BindableObject element)
		{
			return (bool)element.GetValue(UseDefaultShadowProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/Button.xml" path="//Member[@MemberName='SetUseDefaultShadow'][1]/Docs/*" />
		public static void SetUseDefaultShadow(BindableObject element, bool value)
		{
			element.SetValue(UseDefaultShadowProperty, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/Button.xml" path="//Member[@MemberName='UseDefaultShadow']/Docs/*" />
		public static bool UseDefaultShadow(this IPlatformElementConfiguration<Android, FormsElement> config)
		{
			return GetUseDefaultShadow(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/Button.xml" path="//Member[@MemberName='SetUseDefaultShadow'][2]/Docs/*" />
		public static IPlatformElementConfiguration<Android, FormsElement> SetUseDefaultShadow(this IPlatformElementConfiguration<Android, FormsElement> config, bool value)
		{
			SetUseDefaultShadow(config.Element, value);
			return config;
		}
		#endregion
	
		#region RippleColor

		/// <summary>Bindable property for attached property <c>RippleColor</c>.</summary>
		public static readonly BindableProperty RippleColorProperty = BindableProperty.Create("RippleColor", typeof(Color), typeof(Button), default);

		public static Color GetRippleColor(BindableObject element)
		{
			return (Color)element.GetValue(RippleColorProperty);
		}

		public static void SetRippleColor(BindableObject element, Color value)
		{
			element.SetValue(RippleColorProperty, value);
		}

		public static Color GetRippleColor(this IPlatformElementConfiguration<Android, FormsElement> config)
		{
			return GetRippleColor(config.Element);
		}

		public static IPlatformElementConfiguration<Android, FormsElement> SetRippleColor(this IPlatformElementConfiguration<Android, FormsElement> config, Color value)
		{
			SetRippleColor(config.Element, value);
			return config;
		}

		#endregion
	}
}
