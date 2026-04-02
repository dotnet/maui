#nullable disable
namespace Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific
{
	using FormsElement = Maui.Controls.TabbedPage;

	/// <summary>Provides iOS-specific configuration for TabbedPage tab bar translucency.</summary>
	public static class TabbedPage
	{
		/// <summary>Bindable property for <see cref="TranslucencyMode"/>.</summary>
		public static readonly BindableProperty TranslucencyModeProperty =
			BindableProperty.Create("TranslucencyMode",
				typeof(TranslucencyMode), typeof(TabbedPage), TranslucencyMode.Default);

		/// <summary>Gets the tab bar translucency mode on iOS.</summary>
		/// <param name="element">The element to get the value from.</param>
		/// <returns>The translucency mode.</returns>
		public static TranslucencyMode GetTranslucencyMode(BindableObject element)
			=> (TranslucencyMode)element.GetValue(TranslucencyModeProperty);

		/// <summary>Sets the tab bar translucency mode on iOS.</summary>
		/// <param name="element">The element to set the value on.</param>
		/// <param name="value">The translucency mode to apply.</param>
		public static void SetTranslucencyMode(BindableObject element, TranslucencyMode value)
			=> element.SetValue(TranslucencyModeProperty, value);

		/// <summary>Gets the tab bar translucency mode on iOS.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <returns>The translucency mode.</returns>
		public static TranslucencyMode GetTranslucencyMode(
			this IPlatformElementConfiguration<iOS, FormsElement> config)
			=> GetTranslucencyMode(config.Element);

		/// <summary>Sets the tab bar translucency mode on iOS.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <param name="value">The translucency mode to apply.</param>
		/// <returns>The updated platform configuration.</returns>
		public static IPlatformElementConfiguration<iOS, FormsElement> SetTranslucencyMode(
			this IPlatformElementConfiguration<iOS, FormsElement> config, TranslucencyMode value)
		{
			SetTranslucencyMode(config.Element, value);
			return config;
		}
	}
}
