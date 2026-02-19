#nullable disable
using System;

namespace Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific
{
	using FormsElement = Maui.Controls.FlyoutPage;

	/// <summary>Provides Windows-specific configuration for flyout page collapse behavior and pane width.</summary>
	public static class FlyoutPage
	{
		#region CollapsedStyle

		/// <summary>Bindable property for <see cref="CollapseStyle"/>.</summary>
		public static readonly BindableProperty CollapseStyleProperty =
			BindableProperty.CreateAttached("CollapseStyle", typeof(CollapseStyle),
				typeof(FlyoutPage), CollapseStyle.Full);

		/// <summary>Gets the collapse style for the flyout pane on Windows.</summary>
		/// <param name="element">The element to get the collapse style from.</param>
		/// <returns>The collapse style.</returns>
		public static CollapseStyle GetCollapseStyle(BindableObject element)
		{
			return (CollapseStyle)element.GetValue(CollapseStyleProperty);
		}

		/// <summary>Sets the collapse style for the flyout pane on Windows.</summary>
		/// <param name="element">The element to set the collapse style on.</param>
		/// <param name="collapseStyle">The collapse style to set.</param>
		public static void SetCollapseStyle(BindableObject element, CollapseStyle collapseStyle)
		{
			element.SetValue(CollapseStyleProperty, collapseStyle);
		}

		/// <summary>Gets the collapse style for the flyout pane on Windows.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <returns>The collapse style.</returns>
		public static CollapseStyle GetCollapseStyle(this IPlatformElementConfiguration<Windows, FormsElement> config)
		{
			return (CollapseStyle)config.Element.GetValue(CollapseStyleProperty);
		}

		/// <summary>Sets the collapse style for the flyout pane on Windows.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <param name="value">The collapse style to set.</param>
		/// <returns>The updated platform configuration.</returns>
		public static IPlatformElementConfiguration<Windows, FormsElement> SetCollapseStyle(
			this IPlatformElementConfiguration<Windows, FormsElement> config, CollapseStyle value)
		{
			config.Element.SetValue(CollapseStyleProperty, value);
			return config;
		}

		/// <summary>Configures the flyout to use partial collapse mode on Windows.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <returns>The updated platform configuration.</returns>
		public static IPlatformElementConfiguration<Windows, FormsElement> UsePartialCollapse(
			this IPlatformElementConfiguration<Windows, FormsElement> config)
		{
			SetCollapseStyle(config, CollapseStyle.Partial);
			return config;
		}

		#endregion

		#region CollapsedPaneWidth

		/// <summary>Bindable property for attached property <c>CollapsedPaneWidth</c>.</summary>
		public static readonly BindableProperty CollapsedPaneWidthProperty =
			BindableProperty.CreateAttached("CollapsedPaneWidth", typeof(double),
				typeof(FlyoutPage), 48d, validateValue: (bindable, value) => (double)value >= 0, propertyChanged : OnCollapsedPaneWidthChanged);

		/// <summary>Gets the width of the collapsed flyout pane on Windows.</summary>
		/// <param name="element">The element to get the collapsed pane width from.</param>
		/// <returns>The collapsed pane width in device-independent units.</returns>
		public static double GetCollapsedPaneWidth(BindableObject element)
		{
			return (double)element.GetValue(CollapsedPaneWidthProperty);
		}

		static void OnCollapsedPaneWidthChanged(BindableObject bindable, object oldValue, object newValue)
		{
			if (bindable is Microsoft.Maui.Controls.FlyoutPage flyoutPage && flyoutPage.Handler is not null)
			{
				flyoutPage.Handler.UpdateValue(nameof(CollapsedPaneWidthProperty));
			}
		}

		/// <summary>Sets the width of the collapsed flyout pane on Windows.</summary>
		/// <param name="element">The element to set the collapsed pane width on.</param>
		/// <param name="collapsedPaneWidth">The collapsed pane width in device-independent units.</param>
		public static void SetCollapsedPaneWidth(BindableObject element, double collapsedPaneWidth)
		{
			element.SetValue(CollapsedPaneWidthProperty, collapsedPaneWidth);
		}

		/// <summary>Gets the width of the collapsed flyout pane on Windows.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <returns>The collapsed pane width in device-independent units.</returns>
		public static double CollapsedPaneWidth(this IPlatformElementConfiguration<Windows, FormsElement> config)
		{
			return (double)config.Element.GetValue(CollapsedPaneWidthProperty);
		}

		/// <summary>Sets the width of the collapsed flyout pane on Windows.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <param name="value">The collapsed pane width in device-independent units.</param>
		/// <returns>The updated platform configuration.</returns>
		public static IPlatformElementConfiguration<Windows, FormsElement> CollapsedPaneWidth(
			this IPlatformElementConfiguration<Windows, FormsElement> config, double value)
		{
			config.Element.SetValue(CollapsedPaneWidthProperty, value);
			return config;
		}

		#endregion
	}
}
