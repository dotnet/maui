#nullable disable

namespace Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific
{
	using FormsElement = Maui.Controls.FlyoutPage;

	/// <summary>Provides iOS-specific configuration for FlyoutPage shadow effects.</summary>
	public static class FlyoutPage
	{
		#region ApplyShadow
		/// <summary>Bindable property for attached property <c>ApplyShadow</c>.</summary>
		public static readonly BindableProperty ApplyShadowProperty = BindableProperty.Create("ApplyShadow", typeof(bool), typeof(FlyoutPage), false);

		/// <summary>Gets whether a drop shadow is applied to the detail page when the flyout is revealed on iOS.</summary>
		/// <param name="element">The element to get the value from.</param>
		/// <returns><see langword="true"/> if shadow is applied; otherwise, <see langword="false"/>.</returns>
		public static bool GetApplyShadow(BindableObject element)
		{
			return (bool)element.GetValue(ApplyShadowProperty);
		}

		/// <summary>Sets whether a drop shadow is applied to the detail page when the flyout is revealed on iOS.</summary>
		/// <param name="element">The element to set the value on.</param>
		/// <param name="value"><see langword="true"/> to apply shadow; otherwise, <see langword="false"/>.</param>
		public static void SetApplyShadow(BindableObject element, bool value)
		{
			element.SetValue(ApplyShadowProperty, value);
		}

		/// <summary>Sets whether a drop shadow is applied to the detail page when the flyout is revealed on iOS.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <param name="value"><see langword="true"/> to apply shadow; otherwise, <see langword="false"/>.</param>
		/// <returns>The updated platform configuration.</returns>
		public static IPlatformElementConfiguration<iOS, FormsElement> SetApplyShadow(this IPlatformElementConfiguration<iOS, FormsElement> config, bool value)
		{
			SetApplyShadow(config.Element, value);
			return config;
		}

		/// <summary>Gets whether a drop shadow is applied to the detail page when the flyout is revealed on iOS.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <returns><see langword="true"/> if shadow is applied; otherwise, <see langword="false"/>.</returns>
		public static bool GetApplyShadow(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetApplyShadow(config.Element);
		}
		#endregion
	}
}
