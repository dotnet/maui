#nullable disable
namespace Microsoft.Maui.Controls.PlatformConfiguration.GTKSpecific
{
	using FormsElement = Maui.Controls.TabbedPage;

	/// <summary>Controls the tab position on tabbed pages on the GTK platform.</summary>
	public static class TabbedPage
	{
		/// <summary>Bindable property for <see cref="TabPosition"/>.</summary>
		public static readonly BindableProperty TabPositionProperty =
			BindableProperty.Create("TabPosition", typeof(TabPosition),
				typeof(TabbedPage), TabPosition.Default);

		/// <summary>Gets the position of the tabs on the tabbed page.</summary>
		/// <param name="element">The platform-specific element.</param>
		/// <returns>The position of the tabs on the tabbed page.</returns>
		public static TabPosition GetTabPosition(BindableObject element)
		{
			return (TabPosition)element.GetValue(TabPositionProperty);
		}

		/// <summary>Sets the position of the tabs on the tabbed page.</summary>
		/// <param name="element">The platform-specific element.</param>
		/// <param name="tabPosition">The new tab position value.</param>
		public static void SetTabPosition(BindableObject element, TabPosition tabPosition)
		{
			element.SetValue(TabPositionProperty, tabPosition);
		}

		/// <summary>Gets the position of the tabs on the tabbed page.</summary>
		/// <param name="config">The platform-specific configuration.</param>
		/// <returns>The position of the tabs on the tabbed page.</returns>
		public static TabPosition GetTabPosition(
			this IPlatformElementConfiguration<GTK, FormsElement> config)
		{
			return GetTabPosition(config.Element);
		}

		/// <summary>Sets the position of the tabs on the tabbed page.</summary>
		/// <param name="config">The platform-specific configuration.</param>
		/// <param name="value">The new tab position value.</param>
		/// <returns>The updated configuration object.</returns>
		public static IPlatformElementConfiguration<GTK, FormsElement> SetTabPosition(
			this IPlatformElementConfiguration<GTK, FormsElement> config, TabPosition value)
		{
			SetTabPosition(config.Element, value);

			return config;
		}
	}
}
