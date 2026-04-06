#nullable disable
namespace Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific
{
	using FormsElement = Maui.Controls.Page;

	/// <summary>Provides access to the bread crumb bar for pages on the Tizen platform.</summary>
	public static class Page
	{
		#region BreadCrumbName
		/// <summary>Bindable property for attached property <c>BreadCrumb</c>.</summary>
		public static readonly BindableProperty BreadCrumbProperty
			= BindableProperty.CreateAttached("BreadCrumb", typeof(string), typeof(FormsElement), default(string));

		/// <summary>Gets the bread crumb text for the page.</summary>
		/// <param name="page">The page whose bread crumb text to get.</param>
		/// <returns>The bread crumb text.</returns>
		public static string GetBreadCrumb(BindableObject page)
		{
			return (string)page.GetValue(BreadCrumbProperty);
		}

		/// <summary>Sets the bread crumb text for the page.</summary>
		/// <param name="page">The page whose bread crumb text to set.</param>
		/// <param name="value">The bread crumb text.</param>
		public static void SetBreadCrumb(BindableObject page, string value)
		{
			page.SetValue(BreadCrumbProperty, value);
		}

		/// <summary>Gets the bread crumb text for the page.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <returns>The bread crumb text.</returns>
		public static string GetBreadCrumb(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			return GetBreadCrumb(config.Element);
		}

		/// <summary>Sets the bread crumb text for the page.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <param name="value">The bread crumb text.</param>
		/// <returns>The updated platform configuration.</returns>
		public static IPlatformElementConfiguration<Tizen, FormsElement> SetBreadCrumb(this IPlatformElementConfiguration<Tizen, FormsElement> config, string value)
		{
			SetBreadCrumb(config.Element, value);
			return config;
		}
		#endregion
	}
}
