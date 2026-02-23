#nullable disable
namespace Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific
{
	using FormsElement = Maui.Controls.NavigationPage;

	/// <summary>Provides access to the bread crumb bar for navigation pages on the Tizen platform.</summary>
	public static class NavigationPage
	{
		#region HasBreadCrumbsBar
		/// <summary>Bindable property for attached property <c>HasBreadCrumbsBar</c>.</summary>
		public static readonly BindableProperty HasBreadCrumbsBarProperty
			= BindableProperty.CreateAttached("HasBreadCrumbsBar", typeof(bool), typeof(FormsElement), false);

		/// <summary>Returns a Boolean value that tells whether the navigation page has a bread crumb bar.</summary>
		/// <param name="element">The navigation page on the Tizen platform whose font weight icon to get.</param>
		/// <returns><see langword="true"/> if the navigation page has a bread crumb bar. Otherwise, <see langword="false"/>.</returns>
		public static bool GetHasBreadCrumbsBar(BindableObject element)
		{
			return (bool)element.GetValue(HasBreadCrumbsBarProperty);
		}

		/// <summary>Sets whether the navigation page has a bread crumb bar.</summary>
		/// <param name="element">The navigation page on the Tizen platform.</param>
		/// <param name="value"><see langword="true"/> to show a bread crumb bar; otherwise, <see langword="false"/>.</param>
		public static void SetHasBreadCrumbsBar(BindableObject element, bool value)
		{
			element.SetValue(HasBreadCrumbsBarProperty, value);
		}

		/// <summary>Returns a Boolean value that tells whether the navigation page has a bread crumb bar.</summary>
		/// <param name="config">The platform configuration for the navigation page on the Tizen platform whose font weight icon to get.</param>
		/// <returns><see langword="true"/> if the navigation page has a bread crumb bar. Otherwise, <see langword="false"/>.</returns>
		public static bool HasBreadCrumbsBar(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			return GetHasBreadCrumbsBar(config.Element);
		}

		/// <summary>Sets whether the navigation page has a bread crumb bar.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <param name="value"><see langword="true"/> to show a bread crumb bar; otherwise, <see langword="false"/>.</param>
		/// <returns>The updated platform configuration.</returns>
		public static IPlatformElementConfiguration<Tizen, FormsElement> SetHasBreadCrumbsBar(this IPlatformElementConfiguration<Tizen, FormsElement> config, bool value)
		{
			SetHasBreadCrumbsBar(config.Element, value);
			return config;
		}
		#endregion
	}
}
