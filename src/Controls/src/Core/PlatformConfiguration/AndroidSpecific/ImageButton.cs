#nullable disable
namespace Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific
{
	using Microsoft.Maui.Graphics;
	using FormsImageButton = Maui.Controls.ImageButton;

	/// <summary>Android-specific shadow effects for ImageButton controls.</summary>
	public static class ImageButton
	{
		#region Shadow
		/// <summary>Bindable property for attached property <c>IsShadowEnabled</c>.</summary>
		public static readonly BindableProperty IsShadowEnabledProperty = BindableProperty.Create("IsShadowEnabled", typeof(bool), typeof(Maui.Controls.ImageButton), false);

		/// <summary>Gets whether the shadow effect is enabled on Android.</summary>
		/// <param name="element">The image button element.</param>
		/// <returns><see langword="true"/> if shadow is enabled; otherwise, <see langword="false"/>.</returns>
		public static bool GetIsShadowEnabled(BindableObject element)
		{
			return (bool)element.GetValue(IsShadowEnabledProperty);
		}

		/// <summary>Sets whether the shadow effect is enabled on Android.</summary>
		/// <param name="element">The image button element.</param>
		/// <param name="value"><see langword="true"/> to enable shadow; otherwise, <see langword="false"/>.</param>
		public static void SetIsShadowEnabled(BindableObject element, bool value)
		{
			element.SetValue(IsShadowEnabledProperty, value);
		}

		/// <summary>Gets whether the shadow effect is enabled on Android.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <returns><see langword="true"/> if shadow is enabled; otherwise, <see langword="false"/>.</returns>
		public static bool GetIsShadowEnabled(this IPlatformElementConfiguration<Android, FormsImageButton> config)
		{
			return GetIsShadowEnabled(config.Element);
		}

		/// <summary>Sets whether the shadow effect is enabled on Android.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <param name="value"><see langword="true"/> to enable shadow; otherwise, <see langword="false"/>.</param>
		/// <returns>The platform configuration for fluent API chaining.</returns>
		public static IPlatformElementConfiguration<Android, FormsImageButton> SetIsShadowEnabled(this IPlatformElementConfiguration<Android, FormsImageButton> config, bool value)
		{
			SetIsShadowEnabled(config.Element, value);
			return config;
		}

		/// <summary>Bindable property for attached property <c>ShadowColor</c>.</summary>
		public static readonly BindableProperty ShadowColorProperty = BindableProperty.Create("ShadowColor", typeof(Color), typeof(ImageButton), null);

		/// <summary>Gets the shadow color on Android.</summary>
		/// <param name="element">The image button element.</param>
		/// <returns>The shadow color.</returns>
		public static Color GetShadowColor(BindableObject element)
		{
			return (Color)element.GetValue(ShadowColorProperty);
		}

		/// <summary>Sets the shadow color on Android.</summary>
		/// <param name="element">The image button element.</param>
		/// <param name="value">The shadow color.</param>
		public static void SetShadowColor(BindableObject element, Color value)
		{
			element.SetValue(ShadowColorProperty, value);
		}

		/// <summary>Gets the shadow color on Android.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <returns>The shadow color.</returns>
		public static Color GetShadowColor(this IPlatformElementConfiguration<Android, FormsImageButton> config)
		{
			return GetShadowColor(config.Element);
		}

		/// <summary>Sets the shadow color on Android.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <param name="value">The shadow color.</param>
		/// <returns>The platform configuration for fluent API chaining.</returns>
		public static IPlatformElementConfiguration<Android, FormsImageButton> SetShadowColor(this IPlatformElementConfiguration<Android, FormsImageButton> config, Color value)
		{
			SetShadowColor(config.Element, value);
			return config;
		}

		/// <summary>Bindable property for attached property <c>ShadowRadius</c>.</summary>
		public static readonly BindableProperty ShadowRadiusProperty = BindableProperty.Create("ShadowRadius", typeof(double), typeof(ImageButton), 10.0);

		/// <summary>Gets the shadow blur radius on Android.</summary>
		/// <param name="element">The image button element.</param>
		/// <returns>The shadow radius.</returns>
		public static double GetShadowRadius(BindableObject element)
		{
			return (double)element.GetValue(ShadowRadiusProperty);
		}

		/// <summary>Sets the shadow blur radius on Android.</summary>
		/// <param name="element">The image button element.</param>
		/// <param name="value">The shadow radius.</param>
		public static void SetShadowRadius(BindableObject element, double value)
		{
			element.SetValue(ShadowRadiusProperty, value);
		}

		/// <summary>Gets the shadow blur radius on Android.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <returns>The shadow radius.</returns>
		public static double GetShadowRadius(this IPlatformElementConfiguration<Android, FormsImageButton> config)
		{
			return GetShadowRadius(config.Element);
		}

		/// <summary>Sets the shadow blur radius on Android.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <param name="value">The shadow radius.</param>
		/// <returns>The platform configuration for fluent API chaining.</returns>
		public static IPlatformElementConfiguration<Android, FormsImageButton> SetShadowRadius(this IPlatformElementConfiguration<Android, FormsImageButton> config, double value)
		{
			SetShadowRadius(config.Element, value);
			return config;
		}

		/// <summary>Bindable property for attached property <c>ShadowOffset</c>.</summary>
		public static readonly BindableProperty ShadowOffsetProperty = BindableProperty.Create("ShadowOffset", typeof(Size), typeof(VisualElement), Size.Zero);

		/// <summary>Gets the shadow offset on Android.</summary>
		/// <param name="element">The image button element.</param>
		/// <returns>The shadow offset as a Size.</returns>
		public static Size GetShadowOffset(BindableObject element)
		{
			return (Size)element.GetValue(ShadowOffsetProperty);
		}

		/// <summary>Sets the shadow offset on Android.</summary>
		/// <param name="element">The image button element.</param>
		/// <param name="value">The shadow offset as a Size.</param>
		public static void SetShadowOffset(BindableObject element, Size value)
		{
			element.SetValue(ShadowOffsetProperty, value);
		}

		/// <summary>Gets the shadow offset on Android.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <returns>The shadow offset as a Size.</returns>
		public static Size GetShadowOffset(this IPlatformElementConfiguration<Android, FormsImageButton> config)
		{
			return GetShadowOffset(config.Element);
		}

		/// <summary>Sets the shadow offset on Android.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <param name="value">The shadow offset as a Size.</param>
		/// <returns>The platform configuration for fluent API chaining.</returns>
		public static IPlatformElementConfiguration<Android, FormsImageButton> SetShadowOffset(this IPlatformElementConfiguration<Android, FormsImageButton> config, Size value)
		{
			SetShadowOffset(config.Element, value);
			return config;
		}
		#endregion

		#region RippleColor

		/// <summary>Bindable property for attached property <c>RippleColor</c>.</summary>
		public static readonly BindableProperty RippleColorProperty = BindableProperty.Create("RippleColor", typeof(Color), typeof(ImageButton), default);

		public static Color GetRippleColor(BindableObject element)
		{
			return (Color)element.GetValue(RippleColorProperty);
		}

		public static void SetRippleColor(BindableObject element, Color value)
		{
			element.SetValue(RippleColorProperty, value);
		}

		public static Color GetRippleColor(this IPlatformElementConfiguration<Android, FormsImageButton> config)
		{
			return GetRippleColor(config.Element);
		}

		public static IPlatformElementConfiguration<Android, FormsImageButton> SetRippleColor(this IPlatformElementConfiguration<Android, FormsImageButton> config, Color value)
		{
			SetRippleColor(config.Element, value);
			return config;
		}

		#endregion
	}
}
