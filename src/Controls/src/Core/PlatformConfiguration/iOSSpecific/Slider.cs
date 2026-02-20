#nullable disable
namespace Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific
{
	using FormsElement = Maui.Controls.Slider;

	/// <summary>Platform-specific functionality for sliders the iOS platform.</summary>
	public static class Slider
	{
		/// <summary>Bindable property for attached property <c>UpdateOnTap</c>.</summary>
		public static readonly BindableProperty UpdateOnTapProperty = BindableProperty.Create("UpdateOnTap", typeof(bool), typeof(Slider), false);

		/// <summary>Gets whether the slider value updates when the user taps on the track on iOS.</summary>
		/// <param name="element">The element to get the value from.</param>
		/// <returns><see langword="true"/> if tap updates value; otherwise, <see langword="false"/>.</returns>
		public static bool GetUpdateOnTap(BindableObject element)
		{
			return (bool)element.GetValue(UpdateOnTapProperty);
		}

		/// <summary>Sets whether the slider value updates when the user taps on the track on iOS.</summary>
		/// <param name="element">The element to set the value on.</param>
		/// <param name="value"><see langword="true"/> to update on tap; otherwise, <see langword="false"/>.</param>
		public static void SetUpdateOnTap(BindableObject element, bool value)
		{
			element.SetValue(UpdateOnTapProperty, value);
		}

		/// <summary>Gets whether the slider value updates when the user taps on the track on iOS.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <returns><see langword="true"/> if tap updates value; otherwise, <see langword="false"/>.</returns>
		public static bool GetUpdateOnTap(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetUpdateOnTap(config.Element);
		}

		/// <summary>Sets whether the slider value updates when the user taps on the track on iOS.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <param name="value"><see langword="true"/> to update on tap; otherwise, <see langword="false"/>.</param>
		/// <returns>The updated platform configuration.</returns>
		public static IPlatformElementConfiguration<iOS, FormsElement> SetUpdateOnTap(this IPlatformElementConfiguration<iOS, FormsElement> config, bool value)
		{
			SetUpdateOnTap(config.Element, value);
			return config;
		}
	}
}
