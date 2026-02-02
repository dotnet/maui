#nullable disable
namespace Microsoft.Maui.Controls.PlatformConfiguration.macOSSpecific
{
	using FormsElement = Maui.Controls.TabbedPage;

	/// <summary>Provides macOS-specific platform configuration for <see cref="Maui.Controls.TabbedPage"/>.</summary>
	public static class TabbedPage
	{
		#region TabsStyle
		/// <summary>Bindable property for <see cref="TabsStyle"/>.</summary>
		public static readonly BindableProperty TabsStyleProperty = BindableProperty.Create("TabsStyle", typeof(TabsStyle), typeof(TabbedPage), TabsStyle.Default);

		/// <summary>Gets the tabs display style for the tabbed page.</summary>
		/// <param name="element">The element whose tabs style to get.</param>
		/// <returns>The tabs style for the element.</returns>
		public static TabsStyle GetTabsStyle(BindableObject element)
		{
			return (TabsStyle)element.GetValue(TabsStyleProperty);
		}

		/// <summary>Sets the tabs display style for the tabbed page.</summary>
		/// <param name="element">The element whose tabs style to set.</param>
		/// <param name="value">The tabs style to apply.</param>
		public static void SetTabsStyle(BindableObject element, TabsStyle value)
		{
			element.SetValue(TabsStyleProperty, value);
		}

		/// <summary>Gets the tabs display style for the tabbed page.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <returns>The tabs style for the element.</returns>
		public static TabsStyle GetTabsStyle(this IPlatformElementConfiguration<macOS, FormsElement> config)
		{
			return GetTabsStyle(config.Element);
		}

		/// <summary>Sets the tabs display style for the tabbed page.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <param name="value">The tabs style to apply.</param>
		/// <returns>The platform configuration for fluent chaining.</returns>
		public static IPlatformElementConfiguration<macOS, FormsElement> SetShowTabs(this IPlatformElementConfiguration<macOS, FormsElement> config, TabsStyle value)
		{
			SetTabsStyle(config.Element, value);
			return config;
		}

		/// <summary>Sets the tabs to be displayed only during navigation.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <returns>The platform configuration for fluent chaining.</returns>
		public static IPlatformElementConfiguration<macOS, FormsElement> ShowTabsOnNavigation(this IPlatformElementConfiguration<macOS, FormsElement> config)
		{
			SetTabsStyle(config.Element, TabsStyle.OnNavigation);
			return config;
		}

		/// <summary>Sets the tabs to be displayed with the default style.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <returns>The platform configuration for fluent chaining.</returns>
		public static IPlatformElementConfiguration<macOS, FormsElement> ShowTabs(this IPlatformElementConfiguration<macOS, FormsElement> config)
		{
			SetTabsStyle(config.Element, TabsStyle.Default);
			return config;
		}

		/// <summary>Hides the tabs on the tabbed page.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <returns>The platform configuration for fluent chaining.</returns>
		public static IPlatformElementConfiguration<macOS, FormsElement> HideTabs(this IPlatformElementConfiguration<macOS, FormsElement> config)
		{
			SetTabsStyle(config.Element, TabsStyle.Hidden);
			return config;
		}
		#endregion
	}
}
