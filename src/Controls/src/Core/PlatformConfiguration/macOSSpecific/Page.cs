#nullable disable
namespace Microsoft.Maui.Controls.PlatformConfiguration.macOSSpecific
{
	using FormsElement = Maui.Controls.Page;

	/// <summary>Provides macOS-specific platform configuration for <see cref="Maui.Controls.Page"/>.</summary>
	public static class Page
	{
		#region TabsStyle
		/// <summary>Bindable property for attached property <c>TabOrder</c>.</summary>
		public static readonly BindableProperty TabOrderProperty = BindableProperty.Create("TabOrder", typeof(VisualElement[]), typeof(Page), null);

		/// <summary>Gets the tab order for keyboard navigation on the page.</summary>
		/// <param name="element">The element whose tab order to get.</param>
		/// <returns>An array of visual elements in their tab order sequence.</returns>
		public static VisualElement[] GetTabOrder(BindableObject element)
		{
			return (VisualElement[])element.GetValue(TabOrderProperty);
		}

		/// <summary>Sets the tab order for keyboard navigation on the page.</summary>
		/// <param name="element">The element whose tab order to set.</param>
		/// <param name="value">The visual elements in the desired tab order sequence.</param>
		public static void SetTabOrder(BindableObject element, params VisualElement[] value)
		{
			element.SetValue(TabOrderProperty, value);
		}

		/// <summary>Gets the tab order for keyboard navigation on the page.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <returns>An array of visual elements in their tab order sequence.</returns>
		public static VisualElement[] GetTabOrder(this IPlatformElementConfiguration<macOS, FormsElement> config)
		{
			return GetTabOrder(config.Element);
		}

		/// <summary>Sets the tab order for keyboard navigation on the page.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <param name="value">The visual elements in the desired tab order sequence.</param>
		/// <returns>The platform configuration for fluent chaining.</returns>
		public static IPlatformElementConfiguration<macOS, FormsElement> SetTabOrder(this IPlatformElementConfiguration<macOS, FormsElement> config, params VisualElement[] value)
		{
			SetTabOrder(config.Element, value);
			return config;
		}
		#endregion
	}
}
