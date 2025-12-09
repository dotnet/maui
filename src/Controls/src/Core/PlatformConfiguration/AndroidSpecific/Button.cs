#nullable disable
namespace Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific
{
	using Microsoft.Maui.Graphics;
	using FormsElement = Maui.Controls.Button;

	/// <summary>Controls padding and shadows for buttons on the Android platform.</summary>
	public static class Button
	{
		#region UseDefaultPadding
		/// <summary>Bindable property for <see cref="UseDefaultPadding"/>.</summary>
		public static readonly BindableProperty UseDefaultPaddingProperty = BindableProperty.Create("UseDefaultPadding", typeof(bool), typeof(Button), false);

		/// <summary>Returns a Boolean value that tells whether the default padding will be used.</summary>
		/// <param name="element">The Android button for which to get the padding behavior.</param>
		/// <returns>A Boolean value that tells whether the default padding will be used.</returns>
		public static bool GetUseDefaultPadding(BindableObject element)
		{
			return (bool)element.GetValue(UseDefaultPaddingProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/Button.xml" path="//Member[@MemberName='SetUseDefaultPadding'][1]/Docs/*" />
		public static void SetUseDefaultPadding(BindableObject element, bool value)
		{
			element.SetValue(UseDefaultPaddingProperty, value);
		}

		/// <summary>Returns <see langword="true"/> if the button will use the default padding. Otherwise, returns <see langword="false"/>.</summary>
		/// <param name="config">The configuration for the Android button whose padding behavior to check.</param>
		/// <returns><see langword="true"/> if the button will use the default padding. Otherwise, <see langword="false"/>.</returns>
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

		/// <summary>Returns a Boolean value that tells whether the default shadow will be used.</summary>
		/// <param name="element">The Android button for which to get the shadow behavior.</param>
		/// <returns>A Boolean value that tells whether the default shadow will be used.</returns>
		public static bool GetUseDefaultShadow(BindableObject element)
		{
			return (bool)element.GetValue(UseDefaultShadowProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/Button.xml" path="//Member[@MemberName='SetUseDefaultShadow'][1]/Docs/*" />
		public static void SetUseDefaultShadow(BindableObject element, bool value)
		{
			element.SetValue(UseDefaultShadowProperty, value);
		}

		/// <summary>Returns <see langword="true"/> if the button will use the default shadow. Otherwise, returns <see langword="false"/>.</summary>
		/// <param name="config">The configuration for the Android button whose shadow behavior to check.</param>
		/// <returns><see langword="true"/> if the button will use the default shadow. Otherwise, <see langword="false"/>.</returns>
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
