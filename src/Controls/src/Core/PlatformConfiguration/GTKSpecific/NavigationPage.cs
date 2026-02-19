#nullable disable
namespace Microsoft.Maui.Controls.PlatformConfiguration.GTKSpecific
{
	using FormsElement = Maui.Controls.NavigationPage;

	/// <summary>Provides access to the back button icon on navigation pages on the GTK platform.</summary>
	public static class NavigationPage
	{
		/// <summary>Bindable property for attached property <c>BackButtonIcon</c>.</summary>
		public static readonly BindableProperty BackButtonIconProperty =
			BindableProperty.Create("BackButtonIcon", typeof(string),
				typeof(NavigationPage), default(string));

		/// <summary>Gets the icon used for the back button.</summary>
		/// <param name="element">The platform-specific element.</param>
		/// <returns>The icon used for the back button.</returns>
		public static string GetBackButtonIcon(BindableObject element)
		{
			return (string)element.GetValue(BackButtonIconProperty);
		}

		/// <summary>Sets the icon used for the back button.</summary>
		/// <param name="element">The platform-specific element.</param>
		/// <param name="backButtonIcon">The new back button icon value.</param>
		public static void SetBackButtonIcon(BindableObject element, string backButtonIcon)
		{
			element.SetValue(BackButtonIconProperty, backButtonIcon);
		}

		/// <summary>Gets the icon used for the back button.</summary>
		/// <param name="config">The platform-specific configuration.</param>
		/// <returns>The icon used for the back button.</returns>
		public static string GetBackButtonIcon(
			this IPlatformElementConfiguration<GTK, FormsElement> config)
		{
			return GetBackButtonIcon(config.Element);
		}

		/// <summary>Sets the icon used for the back button.</summary>
		/// <param name="config">The platform-specific configuration.</param>
		/// <param name="value">The new back button icon value.</param>
		/// <returns>The updated configuration object.</returns>
		public static IPlatformElementConfiguration<GTK, FormsElement> SetBackButtonIcon(
			this IPlatformElementConfiguration<GTK, FormsElement> config, string value)
		{
			SetBackButtonIcon(config.Element, value);

			return config;
		}
	}
}
